using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField]
    private List<LocationQuest> locationQuests;
    [SerializeField]
    private List<LocationQuestTrigger> locationQuestTriggers;

    private List<Quest> allQuests = new List<Quest>();

    private List<LocationQuestInstance> activeLocationQuests = new List<LocationQuestInstance>(); 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize quests
        allQuests.AddRange(locationQuests);
        Debug.Log("QuestManager initialized with " + allQuests.Count + " quests.");
        for (int i = 0; i < locationQuests.Count; i++)
        {
            LocationQuestInstance questInstance = (LocationQuestInstance)locationQuests[i].Instantiate();
            locationQuestTriggers[i].Initialize(questInstance);
            questInstance.SetCompletionTrigger(locationQuestTriggers[i]);
            locationQuests[i].StartQuest();
            activeLocationQuests.Add(questInstance);
        }
    }

    public void StartQuest(int questIndex)
    {
        if (questIndex >= 0 && questIndex < allQuests.Count)
        {
            allQuests[questIndex].StartQuest();
        }
    }

    public void CompleteQuest(int questIndex)
    {
        if (questIndex >= 0 && questIndex < allQuests.Count)
        {
            allQuests[questIndex].CompleteQuest();
        }
    }

    public List<Quest> GetAllQuests()
    {
        return allQuests;
    }

    public List<Quest> GetActiveQuests()
    {
        return allQuests.FindAll(q => q.GetStatus() == Quest.QuestStatus.Started);
    }
    
    public List<Quest> GetInactiveQuests()
    {
        return allQuests.FindAll(q => q.GetStatus() == Quest.QuestStatus.Inactive);
    }
}
