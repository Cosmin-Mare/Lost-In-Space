using UnityEngine;

public class UIQuestManager : MonoBehaviour
{
    private QuestManager questManager;
    [SerializeField]
    private UIQuest uiQuestPrefab;

    private void Start()
    {
        questManager = QuestManager.Instance;
        DisplayActiveQuests();
    }

    private void DisplayActiveQuests()
    {
        var activeQuests = questManager.GetActiveQuests();
        foreach (var quest in activeQuests)
        {
            uiQuestPrefab.Initialize(quest);
            Instantiate(uiQuestPrefab, transform);
        }
    }
}
