/*
 * ======================================================================================
 * INTERFACE UTILISATEUR DE RECHERCHE DE PARTIES LAN (LAN MENU)
 * ======================================================================================
 * 
 * DESCRIPTION GENERALE :
 * Ce composant gère l'affichage dynamique du menu de recherche et de sélection de 
 * parties sur le réseau local (LAN). Il utilise l'ancien système IMGUI (OnGUI) d'Unity 
 * pour construire une interface centrée et responsive.
 *
 * FONCTIONNEMENT :
 * 1. DÉTECTION & RÉCUPÉRATION :
 *    - Interroge le composant LanDiscovery pour obtenir un instantané (GetHostsSnapshot) 
 *      de tous les hôtes actifs détectés sur le sous-réseau.
 * 
 * 2. RENDU VISUEL & ERGONOMIE (OnGUI) :
 *    - Calcule un conteneur centré représentant 75% de la taille de l'écran.
 *    - Affiche une barre d'en-tête dynamique indiquant le statut de la recherche et 
 *      le nombre de parties actuellement répertoriées.
 *    - Présente chaque hôte disponible sous forme de "carte" personnalisée (GUIStyle) 
 *      affichant le nom de la partie, l'adresse IP et le port.
 *    - Propose un défilement vertical (ScrollView) en cas de nombre élevé de serveurs.
 * 
 * 3. CONNEXION :
 *    - Fournit un bouton "REJOINDRE" pour chaque hôte, permettant d'initier la connexion 
 *      directe (via LanDiscovery.ConnectTo) avec le transport réseau (UnityTransport).
 *
 * REQUIS :
 * - Nécessite une référence valide vers un composant LanDiscovery dans la scène.
 * ======================================================================================
 */
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