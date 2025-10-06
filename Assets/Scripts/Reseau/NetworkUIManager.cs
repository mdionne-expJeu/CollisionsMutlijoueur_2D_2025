using UnityEngine;
using TMPro;
using System;


public class NetworkUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField champsIPHote;
    [SerializeField] private TMP_InputField champsIPClient;

    // Référence au script NetworkIpUtility présent dans la scène sur le gameobject UIMananagerNetwork
    public NetworkIpUtility networkIpUtility;

    //Callback du bouton ReseauLocal. Affichage de l'IP local
    public void SelectionReseauLocal()
    {
        string ip = NetworkIpUtility.GetLocalIPv4();
        champsIPHote.text = ip;
    }

    //Callback du bouton Reseau publique. Affichage de l'IP publique
    public void SelectionReseauPublic()
    {
        //Appel de la coroutine GetPublicIP du script NetworkIpUtility. Entre les (), on indique la fonction
        // qui doit être exécutée lorsqu'une valeur est retournée
        StartCoroutine(networkIpUtility.GetPublicIP(OnReceptionIpPublique));

    }

    //Fonction qui affiche l'IP publique lorsque la coroutine GetPublicIP retourne une valeur
    private void OnReceptionIpPublique(string ip)
    {
        champsIPHote.text = ip;
    }

    //Affiche de l'IP du localHost
    public void SelectionLocalHost()
    {
        string ip = "127.0.0.1";
        champsIPHote.text = ip;
    }

    // Callback du bouton qui permet de créer l'hôte
    public void CreerHote()
    {
        if (champsIPHote.text != "")
            GameManager.singleton.LancementHote(champsIPHote.text);
    }
    
    // Callback du bouton qui permet de créer le client
     public void ClientRejointHote()
    {
        if (champsIPClient.text != "")
            GameManager.singleton.LancementClient(champsIPClient.text);
    }
}
