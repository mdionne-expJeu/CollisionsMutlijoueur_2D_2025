// AppBootstrap.cs (dans la scène Bootstrap)
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

public class AppBootstrap : MonoBehaviour
{
    [SerializeField] string mainMenuSceneName;

    async void Awake()
    {
        DontDestroyOnLoad(gameObject); // l’objet bootstrap peut rester

        // Init UGS une seule fois ici
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Charger le menu
        await SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
    }
}