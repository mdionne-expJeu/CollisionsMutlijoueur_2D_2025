using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class UGSBootstrap : MonoBehaviour
{
    public static UGSBootstrap Instance { get; private set; }

    public static Task InitializationTask { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializationTask = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            string uniqueProfile =
                "Build_" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 12);

            Debug.Log(
                $"[UGS] Initializing with profile: {uniqueProfile}"
            );

            InitializationOptions options =
                new InitializationOptions();

            options.SetProfile(uniqueProfile);

            await UnityServices.InitializeAsync(options);

            Debug.Log(
                $"[UGS] Initialized | " +
                $"State={UnityServices.State} | " +
                $"Profile={AuthenticationService.Instance.Profile} | " +
                $"PlayerId={AuthenticationService.Instance.PlayerId}"
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"[UGS] Initialization failed: {ex}"
            );

            throw;
        }
    }
}