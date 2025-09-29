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

    [SerializeField] private GameObject decouvreLANHote;
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
    public void NavigationPanelHost()
    {
        panelSelectionHostClient.SetActive(false);
        panelHostConfig.SetActive(true);
    

        // Section pour Relay
        //RelayManager.instance.StartCoroutine(RelayManager.instance.ConfigureTransportAndStartNgoAsHost());
    }

    public void CreationPartieHote ()
    {
        panelHostConfig.SetActive(false);
        PanelAttenteServeur.SetActive(true);
        decouvreLANHote.GetComponent<LanDiscovery>().gameName = champsNomPartieHote.text;
        decouvreLANHote.SetActive(true);
        NomPartieHote.text = champsNomPartieHote.text;
        GameManager.singleton.LancementHoteDecouverteLan();
;
    }



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
