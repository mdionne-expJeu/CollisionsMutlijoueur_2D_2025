using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager singleton { get; private set; }
    public GameObject panelDeConnexion;
    public GameObject panelAttente;
    public GameObject balle;
    public Action OnDebutPartie; // Création d'une action auquel d'autres scripts pourront s'abonner.


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
    // Abonnement au callback OnClientConnectedCallback qui lancera la fonction OnNouveauClientConnecte.
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += OnNouveauClientConnecte;
    }

    // Désabonnement du callback OnClientConnectedCallback.
    public override void OnNetworkDespawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback -= OnNouveauClientConnecte;
    }

    /* Fonction qui sera appelée lors du callback OnClientConnectedCallback
    Gestion de l'affichage et du début de la partie en fonction du nombre de clients connectés.
    Si juste un client : c'est l'hôte... on affiche un panneau d'attente
    Si deux client : on lance la partie
    */
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


    /*
    Dans cette fonction, on invoque l'action OnDebutPartie. Tous les scripts abonné à cette action exécuteront
    la fonction qu'ils ont associée à cette action.
    */
    void DebutSimulation()
    {
        OnDebutPartie?.Invoke();

        CreationBalle();

    }



    void Update()
    {
        if (!IsServer) return;
        if (Input.GetKeyDown(KeyCode.Return))
        {
            CreationBalle();
        }
    }

    private void CreationBalle()
    {
        GameObject nouvelleBalle = Instantiate(balle);
        nouvelleBalle.GetComponent<NetworkObject>().Spawn();
        nouvelleBalle.GetComponent<Rigidbody2D>().gravityScale = 1;
        StartCoroutine(DesactivationGravite(nouvelleBalle));
    }
    
    IEnumerator DesactivationGravite(GameObject nouvelleBalle)
    {
        yield return new WaitForSeconds(1f);
        nouvelleBalle.GetComponent<Rigidbody2D>().gravityScale = 0;
    }
}
