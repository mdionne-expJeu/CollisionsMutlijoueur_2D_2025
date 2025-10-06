using System.Net;
using System.Net.Sockets;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))] // Component requis pour que le script fonctionne
public class AfficheIPLocale : MonoBehaviour

{
    /* Affichage de l'IP locale dans le start.
    */
    void Start()
    {
        string localIP = GetLocalIPAddress();
        GetComponent<TextMeshProUGUI>().text = "Mon IP locale : " + localIP;
    }

    /* Fonction qui cherche l'adresse IP Locale
    */
    string GetLocalIPAddress()
    {
        string localIP = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
            {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }
}