using UnityEngine;
using Unity.Netcode;

public class DeplaceJoueurServerAuth : NetworkBehaviour
{
    [Header("Mouvement")]
    [SerializeField] private float vitesse = 5f;       // Unités/s
    [SerializeField] private float inputSendRate = 60f; // Hz d’envoi de l’input au serveur (≈ chaque FixedUpdate)

    private Rigidbody2D rb;

    // Côté client (owner) : on garde le dernier input lu et le dernier envoyé
    private float _lastInputRead = 0f;
    private float _lastInputSent = 0f;
    private float _nextSendTime = 0f;

    // Côté serveur : input courant reçu pour CE joueur
    private float _serverInputX = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Si ce n’est PAS le serveur, on ne simule PAS la physique localement.
        // La position sera mise à jour par NetworkTransform (autorité serveur).
        bool weAreServer = IsServer;
        if (rb != null)
        {
            rb.simulated = weAreServer;      // Physique active uniquement sur le serveur
            rb.isKinematic = !weAreServer;   // Kinematic sur clients pour éviter toute force locale
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; // fluide côté serveur
        }
    }

    void Update()
    {
        // Seul le OWNER lit l’input local
        if (!IsOwner || !IsSpawned) return;

        // Ancien Input System (Project Settings > Input Manager)
        _lastInputRead = Input.GetAxis("Horizontal"); // -1 .. +1

        // On throttle l’envoi pour limiter le trafic réseau
        if (Time.time >= _nextSendTime)
        {
            // On envoie si ça a un peu changé OU de temps en temps même identique
            if (Mathf.Abs(_lastInputRead - _lastInputSent) > 0.01f || _lastInputRead == 0f)
            {
                // Envoi vers le serveur (owner requis par défaut)
                SubmitInputServerRpc(_lastInputRead);
                _lastInputSent = _lastInputRead;
            }

            _nextSendTime = Time.time + (1f / Mathf.Max(1f, inputSendRate));
        }
    }

    void FixedUpdate()
    {
        // Seul le SERVEUR applique la vélocité
        if (!IsServer || !IsSpawned || rb == null) return;

        // Clamp par sécurité (anti-cheat basique + stabilité)
        float clamped = Mathf.Clamp(_serverInputX, -1f, 1f);

        // On modifie seulement l’axe X; on conserve la composante Y actuelle
        Vector2 v = rb.linearVelocity; // Unity 6
        v.x = clamped * vitesse;
        rb.linearVelocity = v;
    }

    [Rpc(SendTo.Server)]
    private void SubmitInputServerRpc(float inputX)
    {
        // Ce RPC est exécuté SUR le serveur, pour ce joueur
        _serverInputX = inputX;
        Debug.Log(inputX);
    }
}
