using UnityEngine;

public abstract class Quest : ScriptableObject
{
    public enum QuestStatus { Inactive, Started, Completed }
    public enum QuestType { Inventory, Interaction, Location }
    public enum QuestImportance { Main, Side }


    [Header("Quest Details")]
    public string questName;
    public string description;
    public QuestImportance importance;


    protected QuestType type;
    private QuestStatus status;
    private bool isAvailable = false;

    public abstract QuestInstance Instantiate();
    public void StartQuest()
    {
        status = QuestStatus.Started;
    }

    public void SetAvailability(bool availability)
    {
        isAvailable = availability;
    }

    public virtual void CompleteQuest()
    {
        status = QuestStatus.Completed;
        Debug.Log("Quest '" + questName + "' completed.");
    }

    public QuestStatus GetStatus()
    {
        return status;
    }

    public new QuestType GetType()
    {
        return type;
    }
}