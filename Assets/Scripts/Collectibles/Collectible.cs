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

    /// <summary>
    /// Resolve a mineable resource from a raycast hit. Ores may carry Collectible
    /// on the hit object, a parent wrapper, or a child mesh collider.
    /// </summary>
    public static Collectible FindFromHit(Transform hit)
    {
        if (hit == null)
        {
            return null;
        }

        Collectible collectible = hit.GetComponent<Collectible>();
        if (collectible == null)
        {
            collectible = hit.GetComponentInParent<Collectible>();
        }

        if (collectible == null)
        {
            collectible = hit.GetComponentInChildren<Collectible>();
        }

        return collectible;
    }

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
        GameObject visual = prefabInstance != null ? prefabInstance : gameObject;

        if (resourceData != null && resourceData.item != null && resourceData.item.prefab != null)
        {
            Vector3 worldPosition = visual.transform.position;
            Vector3 itemPosition = new Vector3(worldPosition.x, worldPosition.y + 0.5f, worldPosition.z);
            Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            Instantiate(resourceData.item.prefab, itemPosition, randomRotation);
            visual.SetActive(false);
        }
        else
        {
            visual.SetActive(false);
        }
    }
}
