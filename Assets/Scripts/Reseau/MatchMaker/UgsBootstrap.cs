// UgsBootstrap.cs
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

public class UgsBootstrap : MonoBehaviour
{
    async void Awake()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
    }
}
