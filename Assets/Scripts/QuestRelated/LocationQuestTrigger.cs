using UnityEngine;

public class LocationQuestTrigger : MonoBehaviour
{
    [SerializeField]
    private Player player;
    private LocationQuestInstance locationQuestInstance;

    public void Initialize(LocationQuestInstance questInstance)
    {
        locationQuestInstance = questInstance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player.gameObject)
        {
            locationQuestInstance.CompleteQuest();
            gameObject.SetActive(false);
        }
    }
}
