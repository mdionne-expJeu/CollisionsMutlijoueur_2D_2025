using UnityEngine;
using Unity.Netcode;

public class Balle : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        if (!IsServer) return;
        Vector2 forceRandom = new Vector2(Random.Range(-10f, 10f),0f);

        GetComponent<Rigidbody2D>().AddForce(forceRandom,ForceMode2D.Impulse);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.tag == "Brique")
        {
            SoundManager.instance.JoueSon_Rpc(0);
            collision.gameObject.GetComponent<NetworkObject>().Despawn(false);
        }

    }
   
    
}
