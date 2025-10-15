using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Scriptable Objects/Resource")]
public class Resource : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Name of the resource")]
    public string resourceName = "New Resource";

    [Tooltip("Associated item for this resource")]
    public Item item;

    public int quantity = 1;
    
    [Header("Properties")]
    [Tooltip("Health points of the resource")]
    public float hp = 100f;

    [Header("Visuals")]
    public GameObject prefab;

}
