using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public GameObject panelDeConnexion;
    public GameObject balle;

    void Start()
    {


    }
    public void LancementHote()
    {
        NetworkManager.Singleton.StartHost();
        panelDeConnexion.SetActive(false);
        StartCoroutine(AttenteJoueur2());
    }

    public void LancementClient()
    {
        NetworkManager.Singleton.StartClient();
        panelDeConnexion.SetActive(false);
    }

    IEnumerator AttenteJoueur2()
    {
        print("Entrée coroutine");
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                DebutSimulation();
                yield break;
            }
        }

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
