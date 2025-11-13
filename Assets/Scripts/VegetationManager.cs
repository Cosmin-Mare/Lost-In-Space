using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class VegetationType
{
    public string name = "New Vegetation Type";
    public Mesh mesh;
    public Material material;
    public Transform spawnPointsParent;
    public float scale = 0.2f;
    public float maxRenderDistance = 20f;

    [HideInInspector] public List<Matrix4x4> matrices = new List<Matrix4x4>();
}

public class VegetationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player;

    [Header("Vegetation Types")]
    [SerializeField] private List<VegetationType> vegetationTypes = new List<VegetationType>();

    private const int BatchSize = 1023;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player not assigned!");
            return;
        }

        foreach (var type in vegetationTypes)
        {
            if (type.spawnPointsParent == null)
            {
                Debug.LogWarning($"Vegetation type '{type.name}' has no spawnPointsParent assigned. Skipping.");
                continue;
            }

            type.matrices.Clear();
            Transform[] spawnPoints = type.spawnPointsParent.GetComponentsInChildren<Transform>();
            int count = 0;

            foreach (Transform spawn in spawnPoints)
            {
                if (spawn == type.spawnPointsParent) continue;

                // Random rotation around up axis (Y)
                Quaternion randomRotation = Quaternion.Euler(
                    spawn.rotation.eulerAngles.x,                  // keep original X
                    Random.Range(0f, 360f),                        // random Y rotation
                    spawn.rotation.eulerAngles.z                   // keep original Z
                );

                // Random scale
                float randomScale = Random.Range(0.15f, 0.2f);
                Vector3 finalScale = Vector3.one * randomScale;

                Matrix4x4 matrix = Matrix4x4.TRS(spawn.position, randomRotation, finalScale);
                type.matrices.Add(matrix);
                count++;
            }

            Debug.Log($"[{type.name}] Added {count} vegetation instances.");
        }
    }


    void Update()
    {
        if (player == null) return;

        foreach (var type in vegetationTypes)
        {
            if (type.mesh == null || type.material == null || type.matrices.Count == 0)
                continue;

            List<Matrix4x4> matricesToDraw = new List<Matrix4x4>();
            Vector3 playerPos = player.transform.position;

            // Cull by distance
            foreach (Matrix4x4 matrix in type.matrices)
            {
                float distance = Vector3.Distance(playerPos, matrix.GetColumn(3));
                if (distance <= type.maxRenderDistance)
                {
                    matricesToDraw.Add(matrix);
                }
            }

            // Draw in batches
            for (int i = 0; i < matricesToDraw.Count; i += BatchSize)
            {
                int length = Mathf.Min(BatchSize, matricesToDraw.Count - i);
                Graphics.DrawMeshInstanced(
                    type.mesh,
                    0,
                    type.material,
                    matricesToDraw.GetRange(i, length)
                );
            }

            Debug.Log($"[{type.name}] Rendering {matricesToDraw.Count} instances this frame.");
        }
    }
}
