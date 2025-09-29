using UnityEngine;
using Unity.Netcode;

public class DeplaceJoueur : NetworkBehaviour
{
    [SerializeField] private float vitesse = 5f; // Vitesse réglable dans l'inspecteur


    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        // Récupère l'entrée horizontale (-1 à gauche, +1 à droite)
        float input = Input.GetAxis("Horizontal");

        // Déplacement uniquement sur l'axe X
       rb.linearVelocity = new Vector2(input * vitesse, rb.linearVelocityY);
    }
}