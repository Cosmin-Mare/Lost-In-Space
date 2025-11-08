using UnityEngine;

public class LocationQuestInstance : QuestInstance
{
    private LocationQuestTrigger completionTrigger;

    public override void setQuestData(Quest quest)
    {
        if(quest is not LocationQuest)
        {
            Debug.LogError("Invalid quest type assigned to LocationQuestInstance.");
            return;
        }
        base.setQuestData(quest);
    }

    public void SetCompletionTrigger(LocationQuestTrigger trigger)
    {
        completionTrigger = trigger;
        completionTrigger.gameObject.SetActive(true);
    }
}