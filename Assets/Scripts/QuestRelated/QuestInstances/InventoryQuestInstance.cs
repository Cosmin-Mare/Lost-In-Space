using UnityEngine;

public class InventoryQuestInstance : QuestInstance
{

    public override void setQuestData(Quest quest)
    {
        if(quest is not InventoryQuest)
        {
            Debug.LogError("Invalid quest type assigned to InventoryQuestInstance.");
            return;
        }
        base.setQuestData(quest);
    }
}