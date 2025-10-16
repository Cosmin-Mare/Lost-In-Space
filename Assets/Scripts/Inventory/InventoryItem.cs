using UnityEngine;

public class InventoryItem
{
    public Item item;
    // quantity stored in this inventory slot
    public int quantity;
    public int inventoryIndex;

    public InventoryItem(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}