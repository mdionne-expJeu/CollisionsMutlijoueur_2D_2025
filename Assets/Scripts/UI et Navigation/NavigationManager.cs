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
    

       
    }

    public void CreationPartieHote ()
    {
        panelHostConfig.SetActive(false);
        PanelAttenteServeur.SetActive(true);
        GameManager.singleton.LancementHoteDecouverteLan();
;
    }



    public void NavigationPanelClient()
    {
        panelSelectionHostClient.SetActive(false);
        panelClientConfig.SetActive(true);

        
    }

    public void CachePanelsConfig()
    {
        panelSelectionHostClient.SetActive(false);
        panelClientConfig.SetActive(false);
        panelHostConfig.SetActive(false);
    }

    public void AfficheAttenteServeur()
    {
        panelSelectionHostClient.SetActive(false);
        PanelAttenteServeur.SetActive(true);
    }
    public void AfficheAttenteClient()
    {
        panelSelectionHostClient.SetActive(false);
        PanellAttenteClient.SetActive(true);
    }
    
    public void AffichePanelServeurLancePartie()
    {
        PanelAttenteServeur.SetActive(false);
        PanelServeurLancePartie.SetActive(true);
    }
}
