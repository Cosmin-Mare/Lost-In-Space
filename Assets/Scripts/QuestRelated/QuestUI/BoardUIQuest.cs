using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardUIQuest : MonoBehaviour
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
