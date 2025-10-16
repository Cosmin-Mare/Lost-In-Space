using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
	[Header("Basic")]
	[Tooltip("Display name for this item")]
	public string itemName = "New Item";

	[Tooltip("Icon or image representing the item (Sprite)")]
	public Sprite image;
	[Tooltip("Prefab associated with this item (if any)")]
	public GameObject prefab;

	[Header("Stacking")]
	[Tooltip("Can multiple of this item stack in one inventory slot?")]
	public bool isStackable = false;

	[Tooltip("Maximum number of items per stack. Ignored when isStackable is false.")]
	public int maxStack = 1;

	// Ensure sensible values in the inspector
	private void OnValidate()
	{
		if (isStackable && maxStack < 1)
		{
			maxStack = 1;
		}

		if (!isStackable)
		{
			maxStack = 1;
		}
	}
}
