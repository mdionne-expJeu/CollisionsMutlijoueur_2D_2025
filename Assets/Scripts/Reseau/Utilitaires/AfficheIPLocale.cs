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

    public string GetLocalIPAddressQuick()
    {
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                // On se "connecte" fictivement à une IP publique (aucune donnée n'est envoyée)
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }
        catch
        {
            return "127.0.0.1";
        }
    }
}