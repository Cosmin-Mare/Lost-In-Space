using UnityEngine;

public class EscapePod : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;

    [SerializeField]
    private GameObject Door;

    [SerializeField]
    private GameObject[] InteriorObjects;

    private bool isDoorOpen = false;
    void Update()
    {
        if (GetDistanceToPlayer() < 4f && isDoorOpen)
        {
            foreach (GameObject obj in InteriorObjects)
            {
                obj.SetActive(true);
            }
        }
        else if(GetDistanceToPlayer() >= 4f && isDoorOpen)
        {
            Door.GetComponent<Animator>().SetBool("IsOpen", false);
        }
        // Debug.Log("Light active: " + Light.activeSelf);
        // Debug.Log("Door is open: " + isDoorOpen);
    }

    private float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, Player.transform.position);
    }
    
    public void OnDoorCloseAnimationHit()
    {
        isDoorOpen = !isDoorOpen;
        foreach (GameObject obj in InteriorObjects)
        {
            obj.SetActive(false);
        }
    }
}
