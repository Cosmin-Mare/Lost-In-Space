using UnityEngine;

/// <summary>
/// Planet-relative movement helpers. CharacterController is world-Y aligned, so
/// walking on a sphere slice must use the planet radial axis instead of world up.
/// </summary>
public static class PlanetMotor
{
    public static Vector3 SurfaceUp(Vector3 position, Vector3 planetCenter)
    {
        Vector3 up = position - planetCenter;
        return up.sqrMagnitude > 1e-8f ? up.normalized : Vector3.up;
    }

    public static Quaternion AlignToUp(Quaternion rotation, Vector3 up)
    {
        if (up.sqrMagnitude < 1e-8f)
        {
            return rotation;
        }

        up.Normalize();
        Vector3 currentUp = rotation * Vector3.up;
        if (currentUp.sqrMagnitude < 1e-8f)
        {
            currentUp = Vector3.up;
        }

        Quaternion align = Quaternion.FromToRotation(currentUp.normalized, up);
        return align * rotation;
    }

    public static Vector3 TangentMove(Vector3 worldInput, Vector3 up, float speed)
    {
        Vector3 move = Vector3.ProjectOnPlane(worldInput, up);
        if (move.sqrMagnitude < 1e-8f)
        {
            return Vector3.zero;
        }

        return move.normalized * speed;
    }

    public static bool IsSelfHit(Transform playerRoot, Transform hit)
    {
        return hit != null && playerRoot != null &&
               (hit == playerRoot || hit.IsChildOf(playerRoot));
    }
}
