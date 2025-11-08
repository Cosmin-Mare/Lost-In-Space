using UnityEngine;

public class InteractionQuestInstance : QuestInstance
{  
    public override void setQuestData(Quest quest)
    {
        if(quest is not InteractionQuest)
        {
            Debug.LogError("Invalid quest type assigned to InteractionQuestInstance.");
            return;
        }
        base.setQuestData(quest);
    }
}