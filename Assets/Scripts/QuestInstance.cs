using UnityEngine;

public abstract class QuestInstance: MonoBehaviour
{
    private Quest questData;

    public virtual void setQuestData(Quest quest)
    {
        questData = quest;
    }

    public void CompleteQuest()
    {
        questData.CompleteQuest();
    }
}