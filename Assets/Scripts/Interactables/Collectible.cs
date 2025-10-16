using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Resource Data")]
    [SerializeField]
    private Resource resourceData;

    [SerializeField]
    private ParticleSystem hitEffect;

    private float currentHp;
    private bool isCollected = false;
    [SerializeField]
    private GameObject prefabInstance;
    [SerializeField]
    private AudioSource hitAudioSource;

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
    }


    private void Update()
    {
        // Update behavior if needed in the future
    }



    // Called when the prefab is mined by raycast
    public void MineResource()
    {
        if (isCollected || resourceData == null) return;

        currentHp -= 25f;
        if (hitEffect != null)
        {
            hitEffect.Play();
        } else
        {
            Debug.LogWarning("Hit effect not assigned in Collectible!");
        }
        if (hitAudioSource != null)
        {
            hitAudioSource.pitch = Random.Range(0.8f, 1.2f);
            hitAudioSource.Play();
        } else
        {
            Debug.LogWarning("Hit audio source not assigned in Collectible!");
        }
        Debug.Log($"Resource HP: {Mathf.Max(0, currentHp)} / {resourceData.hp}");
        if (currentHp <= 0f)
        {
            Debug.Log("Resource depleted, collecting...");
            Collect();
        }
    }

    private void Collect()
    {
        isCollected = true;
        
        // Spawn the collected item prefab
        if (resourceData != null && resourceData.item != null && resourceData.item.prefab != null)
        {
            // Get the world position of the resource prefab
            Vector3 worldPosition = prefabInstance.transform.position;
            Vector3 itemPosition = new Vector3(worldPosition.x, worldPosition.y + 0.5f, worldPosition.z);
            Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            
            // Spawn the item prefab at the root level, using the resource prefab's world position
            GameObject collectedItem = Instantiate(resourceData.item.prefab, itemPosition, randomRotation);
            
            // Set the resource prefab inactive
            prefabInstance.SetActive(false);
        }

    }
}
