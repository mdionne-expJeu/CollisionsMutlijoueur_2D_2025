using UnityEngine;
using TMPro;

public class AfficheIPPublique : MonoBehaviour
{
    public NetworkIpUtility networkIpUtility; // Assigne dans l'inspecteur

    void Start()
    {
        StartCoroutine(networkIpUtility.GetPublicIP(OnReceptionIpPublique));
    }

   private void OnReceptionIpPublique(string ip)
   {
       GetComponent<TextMeshProUGUI>().text = "Mon IP publique : " + ip;
   }
}
