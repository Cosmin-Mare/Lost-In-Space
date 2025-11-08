using UnityEngine;

public class EscapePodDoor : MonoBehaviour
{
    [SerializeField]
    private EscapePod EscapePod;

    public void OnDoorCloseAnimationHit()
    {
        EscapePod.OnDoorCloseAnimationHit();
    }
}
