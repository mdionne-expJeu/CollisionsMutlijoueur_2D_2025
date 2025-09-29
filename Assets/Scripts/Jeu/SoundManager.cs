using UnityEngine;
using Unity.Netcode;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager instance;
    [SerializeField] AudioClip[] sons;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    [Rpc(SendTo.Everyone)]
    public void JoueSon_Rpc(int noSon)
    {
        GetComponent<AudioSource>().PlayOneShot(sons[noSon]);
    }


}


