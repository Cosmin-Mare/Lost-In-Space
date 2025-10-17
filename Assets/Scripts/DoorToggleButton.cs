using UnityEngine;

public class DoorToggleButton : MonoBehaviour, Interactable
{
    [SerializeField] private GameObject door;
    private Animator doorAnimator;
    private Animator buttonAnimator;
    void Start()
    {
        doorAnimator = door.GetComponent<Animator>();
        buttonAnimator = GetComponent<Animator>();
    }
    public void Interact()
    {
        buttonAnimator.SetTrigger("PushButton");
        doorAnimator.SetBool("IsOpen", !doorAnimator.GetBool("IsOpen")); 
    }
}
