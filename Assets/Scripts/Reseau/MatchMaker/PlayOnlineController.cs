// PlayOnlineController.cs
// UTP 2.5.3 — NGO 2.5.0 — Services (Matchmaker/Lobbies/Relay) récents
// Pairage automatique 2 joueurs (Host-Client) sans afficher le Join Code.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// UGS Core + Auth
using Unity.Services.Core;
using Unity.Services.Authentication;

// Matchmaker
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models; // Team, TicketStatusResponse, MatchIdAssignment, MultiplayAssignment

// Lobby
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

// Relay
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

// NGO + UTP
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

// Aliases pour éviter le conflit "Player" (Lobby vs Matchmaker)
using MMPlayer = Unity.Services.Matchmaker.Models.Player;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player; // (non utilisé ici)

public class PlayOnlineController : MonoBehaviour
{
    [Header("Matchmaker")]
    [SerializeField] private string queueName = "Coop2joueurs";
    [SerializeField] private float ticketPollSeconds = 1.0f;

    [Header("Relay")]
    [SerializeField] private ushort maxClientsForHost = 1; // 2 joueurs total → 1 client à accepter

    [Header("Lobby")]
    [SerializeField] private int quickJoinRetryCount = 3;
    [SerializeField] private int quickJoinRetryDelayMs = 1000;

    private bool _isRunning;
    private LobbyBridge _lobbyBridgeRef; // pour re-lire le code côté client lors des retries

    /// <summary>
    /// Appelé par ton bouton "Jouer en ligne".
    /// </summary>
    public async Task RunOnlineFlowAsync()
    {
        if (_isRunning) return;
        _isRunning = true;

        try
        {
            // --- 0) Init UGS + Auth (si pas déjà fait par ta scène Bootstrap) ---
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");

            // --- 1) Matchmaking : créer ticket + attendre assignation (Relay OU Multiplay) ---
            var _ = await CreateTicketAndWaitAsync(queueName, ticketPollSeconds); // on n'utilise pas le contenu pour l'élection

            // --- 2) Lobby : QuickJoin avec retries avant de créer (évite 2 lobbys parallèles) ---
            var lobbyBridge = new LobbyBridge(quickJoinRetryCount, quickJoinRetryDelayMs);
            _lobbyBridgeRef = lobbyBridge;
            bool iAmHost = await lobbyBridge.BecomeHostIfNeededAsync();

            // --- 3) Relay + NGO ---
            if (iAmHost)
            {
                var (joinCode, _) = await RelayCreateHostAsync(maxClientsForHost);
                Debug.Log($"[HOST] Relay JoinCode: {joinCode}");
                await lobbyBridge.SetRelayCodeAsync(joinCode);

                if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
                    NetworkManager.Singleton.StartHost();

                Debug.Log("HOST prêt (JoinCode transmis via Lobby).");
                DebugDiag("After StartHost");
            }
            else
            {
                // lire code depuis le lobby
                var joinCode = await lobbyBridge.WaitRelayCodeAsync();
                Debug.Log($"[CLIENT] Received relayCode from Lobby: {joinCode}");

                // JoinAllocation avec retries si "not found"
                await RelayJoinAsClientWithRetriesAsync(joinCode, retryCount: 5, delayMs: 600);

                if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
                    NetworkManager.Singleton.StartClient();

                Debug.Log("CLIENT connecté (JoinCode reçu via Lobby).");
                DebugDiag("After StartClient");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[PlayOnlineController] Erreur dans le flux: " + ex.Message);
            throw; // laisse l'UI afficher l'erreur si tu veux
        }
        finally
        {
            _isRunning = false;
        }
    }

    // ------------------- MATCHMAKER -------------------

    private async Task<TicketStatusResponse> CreateTicketAndWaitAsync(string queue, float pollSeconds)
    {
        // API moderne: CreateTicketAsync(List<Player>, CreateTicketOptions)
        var players = new List<MMPlayer> { new MMPlayer(AuthenticationService.Instance.PlayerId) };
        var options = new CreateTicketOptions
        {
            QueueName = queue,
            Attributes = new Dictionary<string, object>
            {
                { "region", "na-east" }, // facultatif
                { "canHost", true }      // facultatif
            }
        };

        var created = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        Debug.Log($"[MM] Ticket created: {created.Id}");

        // Poll : gère Relay (MatchIdAssignment) OU Multiplay (MultiplayAssignment)
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(pollSeconds));
            var status = await MatchmakerService.Instance.GetTicketAsync(created.Id);

            switch (status.Value)
            {
                case MultiplayAssignment mp:
                    if (mp.Status == MultiplayAssignment.StatusOptions.Found)
                        return status;
                    if (mp.Status == MultiplayAssignment.StatusOptions.Failed)
                        throw new Exception("Matchmaking failed (Multiplay): " + mp.Message);
                    break;

                case MatchIdAssignment mi:
                    if (mi.Status == MatchIdAssignment.StatusOptions.Found)
                        return status;
                    if (mi.Status == MatchIdAssignment.StatusOptions.Failed ||
                        mi.Status == MatchIdAssignment.StatusOptions.Timeout)
                        throw new Exception("Matchmaking failed (Relay): " + mi.Message);
                    break;

                default:
                    // InProgress → continuer
                    break;
            }
        }
    }

    // ------------------- RELAY + NGO (UTP 2.5.3 / NGO 2.5.0) -------------------

    /// <summary>Hôte : crée l’allocation Relay, configure UTP (forme longue), renvoie JoinCode.</summary>
    private async Task<(string joinCode, Allocation alloc)> RelayCreateHostAsync(ushort maxClients)
    {
        var alloc = await RelayService.Instance.CreateAllocationAsync(maxClients);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(
            alloc.RelayServer.IpV4,
            (ushort)alloc.RelayServer.Port,
            alloc.AllocationIdBytes,
            alloc.Key,
            alloc.ConnectionData,
            alloc.ConnectionData,   // côté hôte: hostConnectionData = ConnectionData
            true                    // DTLS
        );

        var joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
        return (joinCode, alloc);
    }

    /// <summary>Client : essaie JoinAllocation, si "not found" → retries avec attente + relecture lobby.</summary>
    private async Task RelayJoinAsClientWithRetriesAsync(string initialJoinCode, int retryCount, int delayMs)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        string lastTried = null;

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            // Relecture du lobby pour un éventuel nouveau code (si l'hôte a relancé)
            string code = initialJoinCode;
            if (attempt > 1 && _lobbyBridgeRef != null && !string.IsNullOrEmpty(_lobbyBridgeRef.Lobby?.Id))
            {
                try
                {
                    var l = await LobbyService.Instance.GetLobbyAsync(_lobbyBridgeRef.Lobby.Id);
                    if (l.Data != null && l.Data.TryGetValue("relayCode", out var d) && !string.IsNullOrEmpty(d.Value))
                        code = d.Value;
                }
                catch { /* ignore lobby transient errors on retry */ }
            }

            if (string.IsNullOrEmpty(code) || code == lastTried)
            {
                await Task.Delay(delayMs);
                continue;
            }

            lastTried = code;
            Debug.Log($"[CLIENT] Trying JoinAllocation with code='{code}' (attempt {attempt}/{retryCount})");

            try
            {
                var join = await RelayService.Instance.JoinAllocationAsync(code);

                transport.SetRelayServerData(
                    join.RelayServer.IpV4,
                    (ushort)join.RelayServer.Port,
                    join.AllocationIdBytes,
                    join.Key,
                    join.ConnectionData,
                    join.HostConnectionData, // côté client
                    true // DTLS
                );

                Debug.Log($"[CLIENT] Join réussi avec le code {code} (attempt {attempt})");
                return; // succès
            }
            catch (Exception ex)
            {
                var msg = ex.Message ?? string.Empty;
                if (msg.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0 && attempt < retryCount)
                {
                    Debug.LogWarning($"[CLIENT] Join failed (not found). Retry in {delayMs}ms… ({attempt}/{retryCount})");
                    await Task.Delay(delayMs);
                    continue;
                }
                throw; // autre erreur → remonte
            }
        }

        throw new Exception("Join failed: relay code not found after retries.");
    }

    private void DebugDiag(string tag)
    {
        var nm = NetworkManager.Singleton;
        Debug.Log($"[Diag] {tag}: IsServer={nm.IsServer} IsClient={nm.IsClient} IsListening={nm.IsListening} Connected={nm.ConnectedClientsList.Count}");
    }

    // ------------------- LOBBY (pont JoinCode invisible) -------------------

    private class LobbyBridge
    {
        public Lobby Lobby { get; private set; }
        private readonly int _retryCount;
        private readonly int _retryDelayMs;

        public LobbyBridge(int retryCount, int retryDelayMs)
        {
            _retryCount = Mathf.Max(0, retryCount);
            _retryDelayMs = Mathf.Clamp(retryDelayMs, 100, 5000);
        }

        /// <summary>
        /// Essaye QuickJoin plusieurs fois pour rejoindre le lobby existant du premier joueur.
        /// Si tous les QuickJoin échouent, crée un Lobby (ce joueur devient hôte).
        /// </summary>
        public async Task<bool> BecomeHostIfNeededAsync()
        {
            for (int i = 0; i < _retryCount; i++)
            {
                try
                {
                    Lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                    Debug.Log("[Lobby] QuickJoin OK");
                    return false; // a rejoint → client
                }
                catch
                {
                    await Task.Delay(_retryDelayMs);
                }
            }

            Lobby = await LobbyService.Instance.CreateLobbyAsync("Coop2MM", 2);
            Debug.Log("[Lobby] Created as host");
            return true; // a créé → hôte
        }

        public async Task SetRelayCodeAsync(string code)
        {
            var data = new Dictionary<string, DataObject> {
                { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, code) }
            };

            Lobby = await LobbyService.Instance.UpdateLobbyAsync(
                Lobby.Id,
                new UpdateLobbyOptions { Data = data });

            Debug.Log("[Lobby] relayCode set");
        }

        public async Task<string> WaitRelayCodeAsync()
        {
            while (true)
            {
                var l = await LobbyService.Instance.GetLobbyAsync(Lobby.Id);
                if (l.Data != null && l.Data.TryGetValue("relayCode", out var d) && !string.IsNullOrEmpty(d.Value))
                    return d.Value;

                await Task.Delay(400);
            }
        }
    }
}
