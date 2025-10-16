using UnityEngine;

public class AnimatorToPlayerRelay : MonoBehaviour
{
    [SerializeField]
    private Player player;

    public void OnMiningAnimationHit()
    {
        if (player != null)
        {
            player.OnMiningAnimationHit();
        }
        else
        {
            Debug.LogWarning("Player reference not set in AnimatorToPlayerRelay.");
        }
    }

    public void OnPickupAnimationHit()
    {
        if (player != null)
        {
            player.OnPickupAnimationHit();
        }
        else
        {
            Debug.LogWarning("Player reference not set in AnimatorToPlayerRelay.");
        }
    }
    public void OnFootstepAnimationHit()
    {
        if (player != null)
        {
            player.OnFootstepAnimationHit();
        }
        else
        {
            Debug.LogWarning("Player reference not set in AnimatorToPlayerRelay.");
        }
    }
}
