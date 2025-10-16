using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public delegate void InventoryChangeHandler();
    public event InventoryChangeHandler OnInventoryChanged;

    private List<InventoryItem> inventory = new List<InventoryItem>();
    private int maxInventorySize = 4;

    public bool AddItem(Item item, int quantity)
    {
        if (item == null)
        {
            Debug.LogError("Trying to add null item to inventory!");
            return false;
        }

        if (quantity <= 0)
        {
            Debug.Log("Cannot add zero or negative quantity.");
            return false;
        }

        Debug.Log($"Attempting to add {quantity}x {item.itemName} to inventory. Current inventory size: {inventory.Count}/{maxInventorySize}");

        // Non-stackable: each unit occupies its own slot
        if (!item.isStackable)
        {
            int availableSlots = maxInventorySize - inventory.Count;
            int toAdd = Mathf.Min(quantity, availableSlots);
            for (int i = 0; i < toAdd; i++)
            {
                inventory.Add(new InventoryItem(item, 1));
            }

            if (toAdd < quantity)
            {
                Debug.Log($"Not enough inventory slots. Added {toAdd} of {quantity} {item.itemName}.");
                return false;
            }

            Debug.Log($"Added {quantity}x {item.itemName} as non-stackable items. New inventory size: {inventory.Count}/{maxInventorySize}");
            OnInventoryChanged?.Invoke();
            Debug.Log("OnInventoryChanged event invoked.");
            return true;
        }

        Debug.Log($"Item {item.itemName} is stackable, trying to fill existing stacks first...");
        // Stackable: try to fill existing stacks first
        int remaining = quantity;

        for (int i = 0; i < inventory.Count && remaining > 0; i++)
        {
            InventoryItem slot = inventory[i];
            if (slot.item == item)
            {
                int space = item.maxStack - slot.quantity;
                if (space > 0)
                {
                    int adding = Mathf.Min(space, remaining);
                    slot.quantity += adding;
                    remaining -= adding;
                }
            }
        }

        // Create new stacks if needed and if there is room
        while (remaining > 0 && inventory.Count < maxInventorySize)
        {
            int adding = Mathf.Min(item.maxStack, remaining);
            inventory.Add(new InventoryItem(item, adding));
            remaining -= adding;
        }

        if (remaining > 0)
        {
            Debug.Log($"Not enough inventory slots. Could not add {remaining} of {item.itemName}.");
            return false;
        }

        Debug.Log($"Successfully added {quantity}x {item.itemName} to inventory. Final inventory size: {inventory.Count}/{maxInventorySize}");
        OnInventoryChanged?.Invoke();
        Debug.Log("OnInventoryChanged event invoked.");
        return true;
    }

    public bool RemoveItem(Item item, int quantity)
    {
        if (quantity <= 0)
        {
            Debug.Log("Cannot remove zero or negative quantity.");
            return false;
        }

        // Find all slots for this item
        int totalAvailable = 0;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item == item)
                totalAvailable += inventory[i].quantity;
        }

        if (totalAvailable < quantity)
        {
            Debug.Log("Not enough items to remove or item not found!");
            return false;
        }

        int remaining = quantity;
        // Remove from the end so we clear newest stacks first (arbitrary choice)
        for (int i = inventory.Count - 1; i >= 0 && remaining > 0; i--)
        {
            InventoryItem slot = inventory[i];
            if (slot.item != item) continue;

            int deduct = Mathf.Min(slot.quantity, remaining);
            slot.quantity -= deduct;
            remaining -= deduct;

            if (slot.quantity <= 0)
            {
                inventory.RemoveAt(i);
            }
        }

        Debug.Log($"Removed {quantity} of {item.itemName} from inventory.");
        OnInventoryChanged?.Invoke();
        return true;
    }

    public List<InventoryItem> GetInventory()
    {
        return inventory;
    }
}
