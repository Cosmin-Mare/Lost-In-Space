using UnityEditor;
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
        questManager.QuestsUpdated += DisplayActiveQuests;
    }

    private void DisplayActiveQuests()
    {
        var activeQuests = questManager.GetActiveQuests();
        Debug.Log(activeQuests.Count);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var quest in activeQuests)
        {
            uiQuestPrefab.Initialize(quest);
            Instantiate(uiQuestPrefab, transform);
        }
    }
}
