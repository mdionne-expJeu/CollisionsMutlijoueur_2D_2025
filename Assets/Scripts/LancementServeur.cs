using UnityEngine;
using UnityEngine.SceneManagement;

public class LancementServeur : MonoBehaviour
{
    [SerializeField] string NomSceneDepart;
    void Start()
    {
        SceneManager.LoadScene(NomSceneDepart);
    }

   
}
