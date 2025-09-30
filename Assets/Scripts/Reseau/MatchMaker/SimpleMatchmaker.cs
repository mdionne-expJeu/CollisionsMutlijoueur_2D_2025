using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;   // <-- Player, TicketStatusResponse, etc.
using Unity.Services.Authentication;      // <-- AuthenticationService

public class SimpleMatchmaker
{
    /// <summary>
    /// Envoie un ticket dans la queue (ex: "Coop_2Players") puis attend l'assignation.
    /// Retourne le TicketStatusResponse (Relay = MatchIdAssignment, Multiplay = MultiplayAssignment).
    /// </summary>
    public async Task<TicketStatusResponse> FindCoop2Async(string queueName = "Coop2joueurs")
    {
        // 1) Préparer la liste de joueurs pour le ticket (party matchmaking possible)
        var players = new List<Player>
        {
            new Player(AuthenticationService.Instance.PlayerId)
        };

        // 2) Options du ticket (queue + éventuels attributs pour filtrer les pools)
        var options = new CreateTicketOptions
        {
            QueueName = queueName,
            Attributes = new Dictionary<string, object>
            {
                { "region", "na-east" },   // optionnel
                { "canHost", true }        // optionnel
            }
        };

        // 3) Créer le ticket avec la nouvelle API
        var create = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        Debug.Log($"Ticket created: {create.Id}");

        // 4) Poller jusqu'à assignation (gère Relay ou Multiplay selon le type)
        var status = await TicketPolling.WaitForAssignmentAsync(create.Id);
        return status;
    }
}
