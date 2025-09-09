using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.Services.Matchmaker.Models;
using System;

public class GameManager : NetworkBehaviour
{
    public GameObject panelDeConnexion;
    public GameObject panelAttente;
    public GameObject balle;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += OnNouveauClientConnecte;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback -= OnNouveauClientConnecte;
    }

    private void OnNouveauClientConnecte(ulong obj)
    {
        if (!IsServer) return;
   
        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            panelDeConnexion.SetActive(false);
            panelAttente.SetActive(true);
        }
        else if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            panelAttente.SetActive(false);
            DebutSimulation();
        }
    }

    public void LancementHote()
    {
        NetworkManager.Singleton.StartHost();
        panelDeConnexion.SetActive(false);
    }

    public void LancementClient()
    {
        NetworkManager.Singleton.StartClient();
        panelDeConnexion.SetActive(false);
    }



    void DebutSimulation()
    {
        Debug.Log("Début simultation");

        GameObject nouvelleBalle = Instantiate(balle);
        nouvelleBalle.GetComponent<NetworkObject>().Spawn();
        StartCoroutine(DesactivationGravite(nouvelleBalle));

    }

    IEnumerator DesactivationGravite(GameObject nouvelleBalle)
    {
        yield return new WaitForSeconds(1f);
        nouvelleBalle.GetComponent<Rigidbody2D>().gravityScale = 0;
    }

}
