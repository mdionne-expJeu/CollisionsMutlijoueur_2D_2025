using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager singleton { get; private set; }
   
    public GameObject balle;
    public GameObject joueur1;
    public GameObject joueur2;
    public Action OnDebutPartie; // Création d'une action auquel d'autres scripts pourront s'abonner.


    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(gameObject);
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

        if (!IsServer)
        {
            NavigationManager.singleton.AfficheAttenteClient();
            return;
        }

        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
            {
                NavigationManager.singleton.AfficheAttenteServeur();
               
            }
            else if (NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                NavigationManager.singleton.AffichePanelServeurLancePartie();
               
            }
    }

    public void ChargementSceneJeu()
    { 
        NetworkManager.Singleton.SceneManager.LoadScene("LeJeu", LoadSceneMode.Single);
    }

    public void LancementHote(string adresseIP)
    {
        UnityTransport utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(adresseIP, 7777);

        NetworkManager.Singleton.StartHost();
        NavigationManager.singleton.CachePanelsConfig();
    }

    public void LancementClient(string adresseIP)
    {
        UnityTransport utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetConnectionData(adresseIP, 7777);

        NetworkManager.Singleton.StartClient();
        NavigationManager.singleton.CachePanelsConfig();
    }


    /*
    Dans cette fonction, on invoque l'action OnDebutPartie. Tous les scripts abonné à cette action exécuteront
    la fonction qu'ils ont associée à cette action.
    */
    public void DebutSimulation()
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
    public void CreationJoueurs()
    {
        GameObject nouveauJoueur = Instantiate(joueur1);
        nouveauJoueur.GetComponent<NetworkObject>().SpawnWithOwnership(0);

        GameObject nouveauJoueur2 = Instantiate(joueur2);
        nouveauJoueur2.GetComponent<NetworkObject>().SpawnWithOwnership(1);
    }
    public void CreationBalle()
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
