using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQuest : MonoBehaviour
{
    public Quest quest;
    [SerializeField]
    private TextMeshProUGUI questTitleText;
    [SerializeField]
    private TextMeshProUGUI questDescriptionText;

    public void Initialize(Quest q)
    {
        quest = q;
        questTitleText.text = quest.questName;
        questDescriptionText.text = quest.description;
    }
}
