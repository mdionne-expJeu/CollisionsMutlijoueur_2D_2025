// PlayOnlineController.cs
// UTP 2.5.3 — NGO 2.5.0 — Matchmaker/Lobbies/Relay récents
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

// -------- Aliases pour éviter le conflit "Player" --------
using MMPlayer = Unity.Services.Matchmaker.Models.Player;
using LobbyPlayer = Unity.Services.Lobbies.Models.Player; // (non utilisé ici, mais prêt si besoin)

public class PlayOnlineController : MonoBehaviour
{
    [Header("Matchmaker")]
    [SerializeField] private string queueName = "Coop2joueurs";
    [SerializeField] private float ticketPollSeconds = 1.0f;

    [Header("Relay")]
    [SerializeField] private ushort maxClientsForHost = 1; // 2 joueurs total → 1 client à accepter

    // Dans PlayOnlineController.cs
private bool _isRunning;

public async System.Threading.Tasks.Task RunOnlineFlowAsync()
{
    if (_isRunning) return; // anti double-clic
    _isRunning = true;

    try
    {
            // 1) Init UGS + Auth
        var options = new InitializationOptions();
        int nbAlea = UnityEngine.Random.Range(0,200);
        options.SetProfile("test_profile"+ nbAlea);
        await Unity.Services.Core.UnityServices.InitializeAsync(options);
        if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log($"Signed in: {Unity.Services.Authentication.AuthenticationService.Instance.PlayerId}");

        // 2) Matchmaking (ticket + polling)
        var _ = await CreateTicketAndWaitAsync(queueName, ticketPollSeconds);

        // 3) Lobby (créateur = hôte, second = client)
        var lobbyBridge = new LobbyBridge();
        bool iAmHost = await lobbyBridge.BecomeHostIfNeededAsync();

        // 4) Relay + NGO
        if (iAmHost)
        {
            var (joinCode, _) = await RelayCreateHostAsync(maxClientsForHost);
            await lobbyBridge.SetRelayCodeAsync(joinCode);

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
                NetworkManager.Singleton.StartHost();

            Debug.Log("HOST prêt (JoinCode transmis via Lobby).");
        }
        else
        {
            var joinCode = await lobbyBridge.WaitRelayCodeAsync();
            await RelayJoinAsClientAsync(joinCode);

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
                NetworkManager.Singleton.StartClient();

            Debug.Log("CLIENT connecté (JoinCode reçu via Lobby).");
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError("[PlayOnlineController] Erreur dans le flux: " + ex.Message);
        throw; // pour que l’UI puisse afficher l’erreur si on veut
    }
    finally
    {
        _isRunning = false;
    }
}


    // ------------------- MATCHMAKER -------------------

    private async Task<TicketStatusResponse> CreateTicketAndWaitAsync(string queue, float pollSeconds)
    {
        // CreateTicketAsync(List<Player>, CreateTicketOptions) — on utilise MMPlayer (alias)
        var players = new List<MMPlayer> { new MMPlayer(AuthenticationService.Instance.PlayerId) };
        var options = new CreateTicketOptions
        {
            QueueName = queue,
            Attributes = new Dictionary<string, object>
            {
                { "region", "na-east" }, // optionnel
                { "canHost", true }      // optionnel (peut aider si tu affines l’élection)
            }
        };

        var created = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        Debug.Log($"Ticket created: {created.Id}");

        // Poll robuste: gère Relay (MatchIdAssignment) OU Multiplay (MultiplayAssignment)
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
                    // InProgress / autres états → continuer à poll
                    break;
            }
        }
    }

    

    // ------------------- RELAY + NGO (UTP 2.5.3 / NGO 2.5.0) -------------------

    /// <summary>Crée l’allocation Relay côté hôte, configure Unity Transport, renvoie le Join Code.</summary>
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
        true                    // secure (DTLS)
    );

    var joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
    return (joinCode, alloc);
}


    /// <summary>Rejoint une allocation Relay côté client et configure Unity Transport.</summary>
    private async Task RelayJoinAsClientAsync(string joinCode)
{
    var join = await RelayService.Instance.JoinAllocationAsync(joinCode);

    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    transport.SetRelayServerData(
        join.RelayServer.IpV4,
        (ushort)join.RelayServer.Port,
        join.AllocationIdBytes,
        join.Key,
        join.ConnectionData,
        join.HostConnectionData, // côté client: différent de ConnectionData
        true                     // secure (DTLS)
    );
}


    // ------------------- LOBBY (pont JoinCode invisible) -------------------


    private class LobbyBridge
    {
        public Lobby Lobby { get; private set; }

        /// <summary>
        /// Essaie QuickJoin ; sinon crée un Lobby.
        /// Retourne true si ce joueur a créé (hôte), false s’il a rejoint (client).
        /// </summary>
        public async Task<bool> BecomeHostIfNeededAsync()
        {
            try
            {
                Lobby = await LobbyService.Instance.QuickJoinLobbyAsync();   // <-- ici
                return false; // a rejoint → client
            }
            catch
            {
                Lobby = await LobbyService.Instance.CreateLobbyAsync("Coop2MM", 2);  // <-- ici
                return true; // a créé → hôte
            }
        }

        public async Task SetRelayCodeAsync(string code)
        {
            var data = new Dictionary<string, DataObject> {
            { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, code) }
        };

            Lobby = await LobbyService.Instance.UpdateLobbyAsync(         // <-- ici
                Lobby.Id,
                new UpdateLobbyOptions { Data = data });
        }

        public async Task<string> WaitRelayCodeAsync()
        {
            while (true)
            {
                var l = await LobbyService.Instance.GetLobbyAsync(Lobby.Id); // <-- ici
                if (l.Data != null && l.Data.TryGetValue("relayCode", out var d) && !string.IsNullOrEmpty(d.Value))
                    return d.Value;

                await Task.Delay(500);
            }
        }
    }
}
