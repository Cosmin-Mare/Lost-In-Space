using UnityEngine;

class InventoryQuest: Quest
{
    public void Awake()
    {
        type = QuestType.Inventory;
    }

    public override QuestInstance Instantiate()
    {
        GameObject questInstanceObj = new GameObject(questName + " Instance");
        InventoryQuestInstance inventoryQuestInstance = questInstanceObj.AddComponent<InventoryQuestInstance>();
        inventoryQuestInstance.setQuestData(this);
        return inventoryQuestInstance;
    }
}