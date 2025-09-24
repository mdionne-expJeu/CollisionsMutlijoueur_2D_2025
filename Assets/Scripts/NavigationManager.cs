using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager singleton;
    [SerializeField] private GameObject panelSelectionHostClient;
    [SerializeField] private GameObject panelHostConfig;
    [SerializeField] private GameObject panelClientConfig;

    [SerializeField] private GameObject PanelAttenteServeur;
    [SerializeField] private GameObject PanellAttenteClient;

    [SerializeField] private GameObject PanelServeurLancePartie;

    [SerializeField] private GameObject decourverLANHote;
    [SerializeField] private GameObject decourverLANClient;
    


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
        PanelAttenteServeur.SetActive(true);
        decourverLANHote.SetActive(true);
        GameManager.singleton.LancementHoteDecouverteLan();

        // Section pour Relay
        //RelayManager.instance.StartCoroutine(RelayManager.instance.ConfigureTransportAndStartNgoAsHost());
    }

    public void NavigationPanelClient()
    {
        panelSelectionHostClient.SetActive(false);
        panelClientConfig.SetActive(true);
        decourverLANClient.SetActive(true);

        
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
