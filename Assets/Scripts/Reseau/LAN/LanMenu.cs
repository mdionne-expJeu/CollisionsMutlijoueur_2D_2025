using UnityEngine;

public class LanMenu : MonoBehaviour
{
    public LanDiscovery discovery;

    Vector2 _scroll;

    void OnGUI()
    {
        if (!discovery) return;

        // Calcule la taille de la fenêtre (3/4 de l’écran)
        float w = Screen.width * 0.75f;
        float h = Screen.height * 0.75f;

        // Calcule la position pour centrer la fenêtre
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - h) / 2f;

        GUILayout.BeginArea(new Rect(x, y, w, h), "LAN Games", GUI.skin.window);

        var hosts = discovery.GetHostsSnapshot();
        if (hosts.Count == 0)
        {
            GUILayout.Label("Aucune partie trouvée...");
        }
        else
        {
            _scroll = GUILayout.BeginScrollView(_scroll);
            foreach (var hst in hosts)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label($"{hst.gameName} [{hst.ip}:{hst.port}]");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Join", GUILayout.Width(120)))
                {
                    discovery.ConnectTo(hst);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        GUILayout.EndArea();
    }
}
