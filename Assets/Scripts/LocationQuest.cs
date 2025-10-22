using UnityEngine;

[CreateAssetMenu(fileName = "Location Quest", menuName = "Scriptable Objects/Quests/Location Quest")]
public class LocationQuest : Quest
{
    public void Awake()
    {
        type = QuestType.Location;
    }

    public override QuestInstance Instantiate()
    {
        GameObject questInstanceObj = new GameObject(questName + " Instance");
        LocationQuestInstance locationQuestInstance = questInstanceObj.AddComponent<LocationQuestInstance>();
        locationQuestInstance.setQuestData(this);
        return locationQuestInstance;
    }
}