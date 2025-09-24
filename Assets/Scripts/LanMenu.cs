using UnityEngine;

public class LanMenu : MonoBehaviour
{
    public LanDiscovery discovery;

    void OnGUI()
    {
        if (!discovery) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 300), "LAN Games", GUI.skin.window);

        var hosts = discovery.GetHostsSnapshot();
        if (hosts.Count == 0)
        {
            GUILayout.Label("Aucune partie trouvée...");
        }
        else
        {
            foreach (var h in hosts)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"{h.gameName} [{h.ip}:{h.port}]");
                if (GUILayout.Button("Join", GUILayout.Width(80)))
                {
                    discovery.ConnectTo(h); // configure UnityTransport + StartClient()
                }
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndArea();
    }
}
