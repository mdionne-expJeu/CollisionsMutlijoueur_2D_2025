using UnityEngine;
using Unity.Netcode;
using System;

public class InitialisationSceneJeu : NetworkBehaviour
{
  
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;
        GameManager.singleton.CreationJoueurs();
        GameManager.singleton.DebutSimulation();
    }

    

    

    

    
   
    
}
