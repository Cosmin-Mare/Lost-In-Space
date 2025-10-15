using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private InventoryManager inventoryManager;

    [SerializeField]
    private Image[] itemImages;

    private void Awake()
    {
        Debug.Log("InventoryUI Awake called");
        if (inventoryManager == null)
        {
            Debug.LogError("No InventoryManager assigned, attempting to find one...");
            inventoryManager = FindFirstObjectByType<InventoryManager>();
            if (inventoryManager != null)
            {
                Debug.Log("Found InventoryManager automatically");
            }
            else
            {
                Debug.LogError("Could not find InventoryManager in the scene!");
                return;
            }
        }
    }

    private void Start()
    {
        Debug.Log("InventoryUI Start called");
        
        if (itemImages == null || itemImages.Length != 4)
        {
            Debug.LogError($"Image array issue. Array is {(itemImages == null ? "null" : "length: " + itemImages.Length)}");
            return;
        }

        // Log state of image components
        for (int i = 0; i < itemImages.Length; i++)
        {
            if (itemImages[i] == null)
            {
                Debug.LogError($"Image component at slot {i} is null!");
            }
        }

        Debug.Log("Subscribing to InventoryManager events...");
        // Subscribe to inventory changes
        inventoryManager.OnInventoryChanged += UpdateInventoryDisplay;
        
        Debug.Log("Performing initial inventory display update");
        // Initial update of the display
        UpdateInventoryDisplay();
    }

    private void OnEnable()
    {
        Debug.Log("InventoryUI OnEnable called");
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += UpdateInventoryDisplay;
            Debug.Log("Resubscribed to inventory events");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= UpdateInventoryDisplay;
        }
    }

    private void UpdateInventoryDisplay()
    {
        Debug.Log("UpdateInventoryDisplay called");
        var inventory = inventoryManager.GetInventory();
        Debug.Log($"Current inventory has {inventory.Count} items");

        // Clear all images first
        for (int i = 0; i < itemImages.Length; i++)
        {
            itemImages[i].sprite = null;
            itemImages[i].enabled = false;
            Debug.Log($"Cleared slot {i}");
        }

        // Update images with inventory items
        for (int i = 0; i < inventory.Count && i < itemImages.Length; i++)
        {
            var item = inventory[i];
            if (item.item == null)
            {
                Debug.LogError($"Null item found in inventory slot {i}");
                continue;
            }

            if (item.item.image != null)
            {
                itemImages[i].sprite = item.item.image;
                itemImages[i].enabled = true;
                Debug.Log($"Updated slot {i} with image for: {item.item.itemName}");
            }
            else
            {
                Debug.LogWarning($"No image assigned for item: {item.item.itemName} in slot {i}");
            }
        }
    }
}
