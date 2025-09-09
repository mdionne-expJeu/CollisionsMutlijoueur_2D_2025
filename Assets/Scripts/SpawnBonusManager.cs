using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SpawnBonusManager : NetworkBehaviour
{
    [SerializeField] GameObject[] objetsBonus; // Tableau avec les prefabs à instancier/spawner
    [SerializeField] Transform[] positionsBonus; // Tableau contentant des positions pour les objets instanciés/spawnés
    private Coroutine spawnBonus_Coroutine; // Référence à une coroutine


    /* 
    - On désactive ce gameObject si on n'est pas le serveur
    - Abonnement à l'action OnDebutPartie du GameManager
    */
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            gameObject.SetActive(false);
            return;
        }

        GameManager.singleton.OnDebutPartie += OnDebutPartie;
    }

    // Désabonnement à l'action OnDebutPartie du GameManager
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        GameManager.singleton.OnDebutPartie -= OnDebutPartie;
    }

    /* Fonction qui sera appelée lorsque l'action OnDebutPartie du GameManager sera invoquée
    - On lance une coroutine qui va spawner des objets à une fréquence variable
    */
    private void OnDebutPartie()
    {
        if (!IsServer) gameObject.SetActive(false);
        spawnBonus_Coroutine = StartCoroutine(GestionSpawn());
    }

   /* Coroutine qui instancie et spawn des objets à une fréquence variable
    - On instancie
    - On attribue une position aléatoire
    - On spawn l'objet pour qu'il apparaisse sur tous les clients
    */
    IEnumerator GestionSpawn()
    {
        while (true)
        {
            float attente = Random.Range(1f, 10f);
            yield return new WaitForSeconds(attente);

            GameObject nouveauBonus = Instantiate(objetsBonus[Random.Range(0, objetsBonus.Length-1)]);
            nouveauBonus.transform.position = positionsBonus[Random.Range(0, positionsBonus.Length - 1)].position;
            nouveauBonus.GetComponent<NetworkObject>().Spawn();
        }
    }
}
