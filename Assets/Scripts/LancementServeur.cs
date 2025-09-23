using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LancementServeur : MonoBehaviour
{
    [SerializeField] SceneAsset sceneLobby;
    void Start()
    {
        SceneManager.LoadScene(sceneLobby.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
