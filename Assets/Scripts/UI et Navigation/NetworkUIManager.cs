using UnityEngine;
using TMPro;
using System;


public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField champsIPHote;
    [SerializeField] private TMP_InputField champsIPClient;
    public NetworkIpUtility networkIpUtility; // Assigne dans l'inspecteur
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SelectionReseauLocal()
    {
        string ip = NetworkIpUtility.GetLocalIPv4();
        champsIPHote.text = ip;
    }

    public void SelectionReseauPublic()
    {
        StartCoroutine(networkIpUtility.GetPublicIP(OnReceptionIpPublique));

    }

    private void OnReceptionIpPublique(string ip)
    {
        champsIPHote.text = ip;
    }

    public void SelectionLocalHost()
    {
        string ip = "127.0.0.1";
        champsIPHote.text = ip;
    }

    public void CreerHote()
    {
        if (champsIPHote.text != "")
            GameManager.singleton.LancementHote(champsIPHote.text);
    }
    
     public void ClientRejointHote()
    {
        if(champsIPClient.text != "")
            GameManager.singleton.LancementClientRelay();
    }
}
