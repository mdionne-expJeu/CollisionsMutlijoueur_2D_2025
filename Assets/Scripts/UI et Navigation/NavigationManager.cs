using UnityEngine;
using TMPro;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager singleton;
    [SerializeField] private GameObject panelSelectionHostClient;
    [SerializeField] private GameObject panelHostConfig;
    [SerializeField] private GameObject panelClientConfig;

    [SerializeField] private GameObject PanelAttenteServeur;
    [SerializeField] private GameObject PanellAttenteClient;

    [SerializeField] private GameObject PanelServeurLancePartie;

    // Référence au gameObject qui utilise LanDiscovery comme hôte
    [SerializeField] private GameObject decouvreLANHote;

    // Référence au gameObject qui utilise LanDiscovery comme hôte
    [SerializeField] private GameObject decouvreLANClient;
    [SerializeField] private TMP_InputField champsNomPartieHote;
    
    [SerializeField] private TextMeshProUGUI NomPartieHote;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /* Callback du bouton "Hôte" lorsque l'utilisateur choisi d'être hôte. Affiche le panel qui permet de choisir
     le nom de la partie
     */
    public void NavigationPanelHost()
    {
        panelSelectionHostClient.SetActive(false);
        panelHostConfig.SetActive(true);
    }

    /* Callback du bouton "Creer l'hôte" activé par l'utilisateur après avoir choisi un nom de partie
    On affiche la panel d'attente d'un autre joueur
    On change le nom de la partie dans le component LanDiscovery
    On active le gameobject contenant le LanDiscovery comme hôte
    On appelle la fonction du gameManager LancementHoteDecouverteLan();
     */
    public void CreationPartieHote()
    {
        panelHostConfig.SetActive(false);
        PanelAttenteServeur.SetActive(true);
        decouvreLANHote.GetComponent<LanDiscovery>().gameName = champsNomPartieHote.text;
        decouvreLANHote.SetActive(true);
        NomPartieHote.text = champsNomPartieHote.text;
        GameManager.singleton.LancementHoteDecouverteLan();
    }


     /* Callback du bouton "Client" lorsque l'utilisateur choisi d'être client.
     On active le panel client qui contient le script d'affichage des hôtes sur le réseau (LanMenu)
     On active le gameObject qui permet de lancer la recherche d'hôtes sur le réseau (LanDiscovery en mode client)
     */
    public void NavigationPanelClient()
    {
        panelSelectionHostClient.SetActive(false);
        panelClientConfig.SetActive(true);
        decouvreLANClient.SetActive(true);
    }

    public void CachePanelsConfig()
    {
        panelSelectionHostClient.SetActive(false);
        panelClientConfig.SetActive(false);
        panelHostConfig.SetActive(false);
    }

    public void AfficheAttenteServeur()
    {
        panelHostConfig.SetActive(false);
        PanelAttenteServeur.SetActive(true);
    }
    public void AfficheAttenteClient()
    {
        panelClientConfig.SetActive(false);
        PanellAttenteClient.SetActive(true);
    }
    
    public void AffichePanelServeurLancePartie()
    {
        PanelAttenteServeur.SetActive(false);
        PanelServeurLancePartie.SetActive(true);
    }
}
