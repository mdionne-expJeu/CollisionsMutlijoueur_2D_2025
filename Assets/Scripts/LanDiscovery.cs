// Assets/Scripts/LanDiscovery.cs
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class LanDiscovery : MonoBehaviour
{
    public enum Role { Host, Client }
    [Header("Mode")]
    public Role role = Role.Client;

    [Header("Discovery Settings")]
    [Tooltip("Port UDP de discovery (différent du port de jeu/UTP).")]
    public int discoveryPort = 47777;
    [Tooltip("Signature pour filtrer uniquement vos paquets.")]
    public string magic = "NGO_DISCOVERY_V1";
    [Tooltip("Intervalle de broadcast (ms).")]
    public int broadcastIntervalMs = 1000;
    [Tooltip("Durée avant expiration d'un hôte non rafraîchi (s).")]
    public float hostTtlSeconds = 5f;

    [Header("Game/Transport")]
    [Tooltip("Nom lisible de la partie (UI).")]
    public string gameName = "Ma Partie";
    [Tooltip("Port du serveur de jeu (UTP). S'il est à 0, on tentera de lire UnityTransport.")]
    public ushort gamePortOverride = 0;

    CancellationTokenSource _cts;
    UdpClient _sender;
    UdpClient _listener;
    IPEndPoint _broadcastEndPoint;

    readonly Dictionary<string, DiscoveredHost> _hosts = new(); // key = ip:port
    readonly object _lock = new();

    string _sessionId; // pour ignorer ses propres annonces côté client

    [Serializable]
    public class DiscoveryMsg
    {
        public string magic;
        public string gameName;
        public string ip;     // ip de l'hôte vue par le client (remplie côté client à la réception)
        public int port;      // port du serveur UTP
        public string session; // identifiant de l’instance Host (pour ignorer soi-même)
        public long ts;       // timestamp Unix ms (informatif)
    }

    [Serializable]
    public class DiscoveredHost
    {
        public string gameName;
        public string ip;
        public int port;
        public string session;
        public DateTime lastSeenUtc;
        public string Key => $"{ip}:{port}";
    }

    void OnEnable()
    {
        _cts = new CancellationTokenSource();
        _sessionId = Guid.NewGuid().ToString("N");

        if (role == Role.Host)
        {
            StartHostBroadcast(_cts.Token);
        }
        else
        {
            StartClientListen(_cts.Token);
        }
    }

    void OnDisable()
    {
        try { _cts?.Cancel(); } catch { }
        try { _sender?.Close(); } catch { }
        try { _listener?.Close(); } catch { }
        _sender = null;
        _listener = null;
        _cts = null;
    }

    void Update()
    {
        // Expire les hôtes non rafraîchis
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var toRemove = new List<string>();
            foreach (var kv in _hosts)
            {
                if ((now - kv.Value.lastSeenUtc).TotalSeconds > hostTtlSeconds)
                    toRemove.Add(kv.Key);
            }
            foreach (var k in toRemove)
                _hosts.Remove(k);
        }
    }

    // ---------- HOST ----------
    async void StartHostBroadcast(CancellationToken ct)
    {
        // Prépare le sender broadcast
        _sender = new UdpClient();
        _sender.EnableBroadcast = true;
        _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);

        // Détermine le port de jeu (UTP)
        int gamePort = ResolveGamePort();

        // Boucle de broadcast
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var msg = new DiscoveryMsg
                {
                    magic = magic,
                    gameName = gameName,
                    ip = "", // rempli côté client à la réception
                    port = gamePort,
                    session = _sessionId,
                    ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                string json = JsonUtility.ToJson(msg);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await _sender.SendAsync(data, data.Length, _broadcastEndPoint);
                print("Envoie présence");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LanDiscovery] Broadcast error: {e.Message}");
            }

            try { await Task.Delay(broadcastIntervalMs, ct); }
            catch (TaskCanceledException) { break; }
        }
    }

    int ResolveGamePort()
    {
        if (gamePortOverride != 0)
            return gamePortOverride;

        var utp = NetworkManager.Singleton
            ? NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport
            : null;

        if (utp != null)
        {
#if UNITY_6000_0_OR_NEWER || UNITY_2022_3_OR_NEWER
            return utp.ConnectionData.Port; // UTP v2/v3
#else
            return utp.ConnectionData.Port;
#endif
        }
        // Valeur commune par défaut si rien n’est configuré
        return 7777;
    }

    // ---------- CLIENT ----------
   
async void StartClientListen(CancellationToken ct)
{
    try
    {
        _listener = new UdpClient(discoveryPort);
        _listener.EnableBroadcast = true;
    }
    catch (Exception e)
    {
        Debug.LogError($"[LanDiscovery] Impossible d'ouvrir le port {discoveryPort}: {e.Message}");
        return;
    }

    Task<UdpReceiveResult> receiveTask = _listener.ReceiveAsync(); // une seule attente partagée

    while (!ct.IsCancellationRequested)
    {
        try
        {
            // Attendre soit la réception, soit un “tick” de 1 seconde
            var completed = await Task.WhenAny(receiveTask, Task.Delay(1000, ct));

            if (completed == receiveTask)
            {
                // Un paquet est arrivé
                var result = receiveTask.Result;

                // Redémarrer immédiatement l’attente pour le prochain paquet
                receiveTask = _listener.ReceiveAsync();

                string json = Encoding.UTF8.GetString(result.Buffer);
                var msg = JsonUtility.FromJson<DiscoveryMsg>(json);
                if (msg == null) continue;
                if (msg.magic != magic) continue;
                if (msg.port <= 0 || msg.port > 65535) continue;
                if (msg.session == _sessionId) continue; // ignorer ses propres annonces

                string ip = result.RemoteEndPoint.Address.ToString();
                var host = new DiscoveredHost
                {
                    gameName = msg.gameName,
                    ip = ip,
                    port = msg.port,
                    session = msg.session,
                    lastSeenUtc = DateTime.UtcNow
                };

                lock (_lock) { _hosts[host.Key] = host; } // dédup + refresh
            }
            else
            {
                // Tick (aucun paquet pendant ~1s) → ne crée PAS un nouveau ReceiveAsync ici.
                // On laisse "receiveTask" vivant pour la prochaine itération.
            }
        }
        catch (ObjectDisposedException) { break; }         // socket fermé (OnDisable)
        catch (TaskCanceledException) { if (ct.IsCancellationRequested) break; }
        catch (Exception e) { Debug.LogWarning($"[LanDiscovery] Listen error: {e.Message}"); }
    }
}



    // ---------- API ----------
    /// <summary>Retourne un snapshot trié (par nom puis ip) des hôtes découverts.</summary>
    public List<DiscoveredHost> GetHostsSnapshot()
    {
        lock (_lock)
        {
            var list = new List<DiscoveredHost>(_hosts.Values);
            list.Sort((a, b) =>
            {
                int n = string.Compare(a.gameName, b.gameName, StringComparison.OrdinalIgnoreCase);
                if (n != 0) return n;
                return string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);
            });
            Debug.Log("Nombre d'hotes trouvés = " + list.Count);
            return list;

        }
    }

    /// <summary>Configure UnityTransport et démarre le client vers l'hôte choisi.</summary>
    public bool ConnectTo(DiscoveredHost host)
    {
        if (host == null) return false;
        var nm = NetworkManager.Singleton;
        if (!nm)
        {
            Debug.LogError("[LanDiscovery] Aucun NetworkManager dans la scène.");
            return false;
        }

        var utp = nm.NetworkConfig.NetworkTransport as UnityTransport;
        if (utp == null)
        {
            Debug.LogError("[LanDiscovery] Transport attendu: UnityTransport.");
            return false;
        }

        // Configure l'adresse et le port avant StartClient
        utp.SetConnectionData(host.ip, (ushort)host.port);

        bool ok = nm.StartClient();
        if (!ok)
            Debug.LogError("[LanDiscovery] Échec StartClient().");
        return ok;
    }
}
