using UnityEngine;
using Unity.Netcode;

public class NetworkDiag : MonoBehaviour
{
    void OnEnable()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback  += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }
    void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback  -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    void Start()
    {
        var nm = NetworkManager.Singleton;
        Debug.Log($"[Diag] Before start: IsServer={nm.IsServer} IsClient={nm.IsClient} IsListening={nm.IsListening}");
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[Diag] OnClientConnected (SERVER sees) clientId={clientId}  Total={NetworkManager.Singleton.ConnectedClientsList.Count}");
    }

    void OnClientDisconnected(ulong clientId)
    {
        var reason = NetworkManager.Singleton.DisconnectReason;
        Debug.LogWarning($"[Diag] OnClientDisconnected clientId={clientId} reason='{reason}'");
    }
}
