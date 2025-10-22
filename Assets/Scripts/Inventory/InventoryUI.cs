using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private InventoryManager inventoryManager;

    [SerializeField]
    private Image[] itemImages;
    [SerializeField]
    private TextMeshProUGUI[] itemCounts;

    private void Awake()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("No InventoryManager assigned, attempting to find one...");
            inventoryManager = FindFirstObjectByType<InventoryManager>();
            if (inventoryManager != null)
            {
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
        // Subscribe to inventory changes
        inventoryManager.OnInventoryChanged += UpdateInventoryDisplay;
        
        // Initial update of the display
        UpdateInventoryDisplay();
    }

    private void OnEnable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += UpdateInventoryDisplay;
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
        var inventory = inventoryManager.GetInventory();

        // Clear all images first
        for (int i = 0; i < itemImages.Length; i++)
        {
            itemImages[i].sprite = null;
            itemImages[i].enabled = false;
            if (itemCounts != null && i < itemCounts.Length && itemCounts[i] != null)
            {
                itemCounts[i].text = "";
            }
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
                if (itemCounts != null && i < itemCounts.Length && itemCounts[i] != null)
                {
                    itemCounts[i].text = item.quantity > 1 ? "x" + item.quantity.ToString() : "";
                }
                Debug.Log($"Updated slot {i} with image for: {item.item.itemName}");
            }
            else
            {
                Debug.LogWarning($"No image assigned for item: {item.item.itemName} in slot {i}");
            }
        }
    }
}
