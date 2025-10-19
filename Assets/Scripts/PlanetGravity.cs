using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    public static PlanetGravity Instance; // Singleton

    public float gravity = -9.81f; // Gravity toward planet center

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Pull a body toward the planet
    public void Attract(Rigidbody body)
    {
        Vector3 gravityDirection = (body.position - transform.position).normalized;
        body.AddForce(gravityDirection * gravity, ForceMode.Acceleration);
    }
}
