using UnityEngine;

public class LanMenu : MonoBehaviour
{
    public LanDiscovery discovery;

    Vector2 _scroll;

    void OnGUI()
    {
        if (!discovery) return;

        // Configuration du style général de la fenêtre
        GUI.skin.window.fontSize = 16;
        GUI.skin.window.fontStyle = FontStyle.Bold;

        // Dimensions et centrage (75% de l'écran)
        float w = Screen.width * 0.75f;
        float h = Screen.height * 0.75f;
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - h) / 2f;

        // Conteneur principal
        GUILayout.BeginArea(new Rect(x, y, w, h), "  RECHERCHE DE PARTIES LAN", GUI.skin.window);

        GUILayout.Space(10);

        var hosts = discovery.GetHostsSnapshot();

        // En-tête / Barre de statut
        GUILayout.BeginHorizontal(GUI.skin.box);
        GUILayout.Label($"<b>Statut :</b> Recherche en cours...", GUILayout.Height(25));
        GUILayout.FlexibleSpace();
        GUILayout.Label($"<b>Parties trouvées :</b> {hosts.Count}", GUILayout.Height(25));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (hosts.Count == 0)
        {
            // Message si aucune partie n'est disponible
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("<size=16><i>Aucune partie disponible sur le réseau local...</i></size>");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else
        {
            // Styles personnalisés pour les éléments de la liste
            GUIStyle cardStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(15, 15, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };

            _scroll = GUILayout.BeginScrollView(_scroll);

            foreach (var hst in hosts)
            {
                // Carte d'information de l'hôte
                GUILayout.BeginHorizontal(cardStyle, GUILayout.Height(50));

                // Informations de la partie
                GUILayout.Label($"🎮 {hst.gameName}  <color=#888888>({hst.ip}:{hst.port})</color>", labelStyle, GUILayout.Height(30));

                GUILayout.FlexibleSpace();

                // Bouton de connexion
                if (GUILayout.Button("REJOINDRE", buttonStyle, GUILayout.Width(130), GUILayout.Height(30)))
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