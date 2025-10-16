using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField]
    private Item itemData;
    
    public void Pickup(){
        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
        if (inventory != null && itemData != null)
        {
            bool added = inventory.AddItem(itemData, 1);
            if (!added)
            {
                Debug.Log("Inventory full or could not add item.");
            }
        }
        else
        {
            Debug.LogError("InventoryManager or Resource item not found!");
        }

        // Instead of destroying immediately, we'll just hide this object
        gameObject.SetActive(false);
        
        // Optional: Destroy after a delay if you don't need these objects anymore
        Destroy(gameObject, 0.1f);
    }
}
