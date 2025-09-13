using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager singleton;
    [SerializeField] private GameObject panelSelectionHostClient;
    [SerializeField] private GameObject panelHostConfig;
    [SerializeField] private GameObject panelClientConfig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created


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
}
