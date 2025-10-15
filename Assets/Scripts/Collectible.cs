using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Resource Data")]
    [SerializeField]
    private Resource resourceData;

    [Header("Spawn Settings")]
    [Tooltip("Where to spawn the resource's prefab when instantiated")]
    [SerializeField]
    private Transform spawnPoint;

    [Header("HP Bar UI")]
    [Tooltip("Progress bar to represent HP (should be a UnityEngine.UI.Image with fillAmount)")]
    [SerializeField]
    private UnityEngine.UI.Image hpBar;

    [SerializeField]
    private GameObject template;

    private float currentHp;
    private bool isCollected = false;
    private GameObject spawnedPrefabInstance;



    private void Awake()
    {
        // Spawn the prefab at the spawnPoint if set, otherwise at this object's position
        if (resourceData != null && resourceData.prefab != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            spawnedPrefabInstance = Instantiate(resourceData.prefab, spawnPos, spawnRot);
        }
        template.SetActive(false);
    }

    private void Start()
    {
        if (resourceData != null)
        {
            currentHp = resourceData.hp;
        }
        else
        {
            Debug.LogError("Resource data not assigned to Collectible!");
        }

        if (hpBar != null && resourceData != null)
        {
            hpBar.fillAmount = 1f;
        }
    }


    private void Update()
    {
        if (InputManager.Instance != null)
        {
            // Check for Fire action (mouse click) using the Input System
            if (InputManager.Instance.GetFire())
            {
                Camera cam = Camera.main;
                if (cam == null)
                {
                    Debug.LogWarning("No main camera found for mining raycast.");
                    return;
                }
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit hit;
                float maxDistance = 15f;
                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    if (spawnedPrefabInstance != null && (hit.transform == spawnedPrefabInstance.transform || hit.transform.IsChildOf(spawnedPrefabInstance.transform)))
                    {
                        Debug.Log($"Hit resource prefab: {spawnedPrefabInstance.name}, subtracting HP.");
                        MineResource();
                    }
                    else
                    {
                        Debug.Log($"Raycast hit: {hit.transform.name}, but it's not the resource prefab.");
                    }
                }
                else
                {
                    Debug.Log("Raycast did not hit anything.");
                }
            }
        }
    }



    // Called when the prefab is mined by raycast
    private void MineResource()
    {
        if (isCollected || resourceData == null) return;

        currentHp -= 25f;
        Debug.Log($"Resource HP: {Mathf.Max(0, currentHp)} / {resourceData.hp}");
        if (hpBar != null && resourceData != null)
        {
            hpBar.fillAmount = Mathf.Clamp01(currentHp / resourceData.hp);
        }
        if (currentHp <= 0f)
        {
            Debug.Log("Resource depleted, collecting...");
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;
        // Try to add to inventory
        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
        if (inventory != null && resourceData.item != null)
        {
            bool added = inventory.AddItem(resourceData.item, resourceData.quantity);
            if (!added)
            {
                Debug.Log("Inventory full or could not add item.");
            }
        }
        else
        {
            Debug.LogError("InventoryManager or Resource item not found!");
        }

        // Destroy the spawned prefab instance after collection
        if (spawnedPrefabInstance != null)
        {
            Destroy(spawnedPrefabInstance);
        }

        Destroy(gameObject);
    }
}
