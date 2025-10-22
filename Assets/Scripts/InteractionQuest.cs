using UnityEngine;

class InteractionQuest : Quest
{
    public override QuestInstance Instantiate()
    {
        GameObject questInstanceObj = new GameObject(questName + " Instance");
        InteractionQuestInstance interactionQuestInstance = questInstanceObj.AddComponent<InteractionQuestInstance>();
        interactionQuestInstance.setQuestData(this);
        return interactionQuestInstance;
    }
}