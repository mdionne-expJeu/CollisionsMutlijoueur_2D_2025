using UnityEngine;
using TMPro;
using System;


public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField champsIP;
    public NetworkIpUtility networkIpUtility; // Assigne dans l'inspecteur
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SelectionReseauLocal()
    {
        string ip = NetworkIpUtility.GetLocalIPv4();
        champsIP.text = ip;
    }

    public void SelectionReseauPublic()
    {
        StartCoroutine(networkIpUtility.GetPublicIP(OnReceptionIpPublique));
       
    }

    private void OnReceptionIpPublique(string ip)
    {
        champsIP.text = ip;
    }

    public void SelectionLocalHost()
    {
        string ip = "127.0.0.1";
        champsIP.text = ip;
    }
}
