using UnityEngine;
using Unity.Netcode;

public class Brique : NetworkBehaviour
{
    public override void OnNetworkDespawn()
    {

        gameObject.SetActive(false);
    }
}
