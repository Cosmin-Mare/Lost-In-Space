using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    private PlanetGravity planet;

    private Rigidbody rb;

    void Start()
    {
        planet = PlanetGravity.Instance;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Turn off Unity’s gravity
    }

    void FixedUpdate()
    {
        planet.Attract(rb);
    }
}
