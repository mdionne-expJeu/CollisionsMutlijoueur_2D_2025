using System;
using System.Threading.Tasks;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;

public static class TicketPolling
{
    /// <summary>
    /// Poll le statut du ticket jusqu'à obtenir une assignation valide.
    /// Gère automatiquement Relay (MatchIdAssignment) ET Multiplay (MultiplayAssignment).
    /// </summary>
    public static async Task<TicketStatusResponse> WaitForAssignmentAsync(string ticketId, float pollSeconds = 1f)
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(pollSeconds));
            var status = await MatchmakerService.Instance.GetTicketAsync(ticketId);

            switch (status.Value)
            {
                case MultiplayAssignment mp:
                    if (mp.Status == MultiplayAssignment.StatusOptions.Found) return status;
                    if (mp.Status == MultiplayAssignment.StatusOptions.Failed)
                        throw new Exception("Matchmaking failed (Multiplay): " + mp.Message);
                    break;

                case MatchIdAssignment mi:
                    if (mi.Status == MatchIdAssignment.StatusOptions.Found) return status;
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
}
