using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class PaintedMarker : MonoBehaviour
{
    public GameObject sourcePrefabAsset;
}

#if UNITY_EDITOR
[EditorTool("Mesh Painter Tool")]
public class MeshPainterTool : EditorTool
{
    GUIContent iconContent;

    [Header("References")]
    public GameObject prefabToPaint;
    public List<GameObject> avoidPrefabs = new List<GameObject>();
    public Transform parentTransform; // NEW: Parent for painted objects

    [Header("Settings")]
    public LayerMask layerMask = ~0;
    public float brushRadius = 1f;
    public int density = 1;
    public bool alignToNormal = true;
    public Vector2 randomScale = new Vector2(1f, 1f);
    public Vector2 randomRotationY = new Vector2(0f, 360f);
    public Vector3 rotationOffset = Vector3.zero; // XYZ rotation offset

    public bool paintOnMouseDrag = true;
    public bool showDebug = true;

    [Header("Spacing")]
    public float minDistance = 1f;
    public LayerMask ignoreForMinDistance = 0; // e.g., Planet layer

    // Visual debug
    public Color validColor = new Color(0, 1, 0, 0.5f);
    public Color invalidColor = new Color(1, 0, 0, 0.5f);

    private bool showSettings = true;
    private float lastPaintTime;
    private const float paintCooldown = 0.03f;

    void OnEnable()
    {
        SceneView.duringSceneGui += RepaintScene;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= RepaintScene;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        // Re-hook the SceneView delegate when exiting Play Mode
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            SceneView.duringSceneGui -= RepaintScene; // prevent double subscription
            SceneView.duringSceneGui += RepaintScene;
        }
    }

    void RepaintScene(SceneView sv) => sv.Repaint();

    public override GUIContent toolbarIcon => iconContent ??= new GUIContent("🖌️ Paint", "Paint meshes aligned to surface normals");

    public override void OnToolGUI(EditorWindow window)
    {
        
        if (!(window is SceneView)) return;
        Event e = Event.current;


        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 330, 480), GUI.skin.box);
        showSettings = EditorGUILayout.Foldout(showSettings, "Mesh Painter Settings");
        if (showSettings)
        {
            prefabToPaint = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToPaint, typeof(GameObject), false);
            parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform", parentTransform, typeof(Transform), true);
            layerMask = LayerMaskField("Layer Mask", layerMask);
            brushRadius = EditorGUILayout.FloatField("Brush Radius", brushRadius);
            density = EditorGUILayout.IntSlider("Density", density, 1, 20);
            alignToNormal = EditorGUILayout.Toggle("Align to Normal", alignToNormal);
            rotationOffset = EditorGUILayout.Vector3Field("Z Offset", rotationOffset);
            randomScale = EditorGUILayout.Vector2Field("Random Scale", randomScale);
            randomRotationY = EditorGUILayout.Vector2Field("Random Y Rot", randomRotationY);
            paintOnMouseDrag = EditorGUILayout.Toggle("Paint On Drag", paintOnMouseDrag);
            showDebug = EditorGUILayout.Toggle("Show Debug", showDebug);
            minDistance = EditorGUILayout.FloatField("Min Distance", minDistance);
            ignoreForMinDistance = LayerMaskField("Ignore Layers", ignoreForMinDistance);
        }
        GUILayout.EndArea();
        Handles.EndGUI();

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            if (showDebug) Debug.Log("[MeshPainter] Raycast did NOT hit anything.");
            return;
        }

        Vector3 candidate = hit.point;
        bool valid = IsPositionValid(candidate, out List<Collider> blockingColliders);

        Handles.color = valid ? validColor : invalidColor;
        Handles.DrawWireDisc(candidate, hit.normal, brushRadius);

        if (!valid && showDebug)
        {
            foreach (var c in blockingColliders)
            {
                Vector3 closest = c.ClosestPoint(candidate);
                Handles.color = Color.red;
                Handles.DrawSolidDisc(closest, Vector3.up, 0.05f);
            }
        }


        // --- Painting ---
        if (prefabToPaint != null)
        {
            bool mousePressed = e.type == EventType.MouseDown && e.button == 0;
            bool mouseDragged = paintOnMouseDrag && e.type == EventType.MouseDrag && e.button == 0;

            if (mousePressed || mouseDragged)
            {
                if (valid)
                {
                    Debug.Log("[MeshPainter] Painting at valid position...");
                    PaintAt(hit);
                    lastPaintTime = Time.realtimeSinceStartup;

                    // Force SceneView to repaint so we see new objects immediately
                    SceneView.RepaintAll();

                    // Consume event so Unity doesn’t also select objects
                    e.Use();
                }
                else
                {
                    Debug.Log("[MeshPainter] Position invalid, not painting.");
                    e.Use();
                }
            }
        }

    }

    private void PaintAt(RaycastHit centerHit)
    {
        for (int i = 0; i < density; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * brushRadius;
            Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y);
            Vector3 start = centerHit.point + offset + Vector3.up * 3f;

            if (!Physics.Raycast(start, Vector3.down, out RaycastHit hit, 10f, layerMask)) continue;
            if (!IsPositionValid(hit.point, out _)) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPaint);
            if (!instance) continue;
            Debug.Log("CREATED INSTANCE");

            Undo.RegisterCreatedObjectUndo(instance, "Paint Mesh");
            instance.transform.position = hit.point;

            if (alignToNormal)
            {
                instance.transform.up = hit.normal;
            }

            // Apply user rotation offset (XYZ)
            instance.transform.Rotate(rotationOffset, Space.Self);

            // Apply random Y rotation (same as before)
            instance.transform.Rotate(Vector3.forward, Random.Range(randomRotationY.x, randomRotationY.y), Space.Self);

            float s = Random.Range(randomScale.x, randomScale.y);
            instance.transform.localScale = Vector3.one * s;

            if (parentTransform) instance.transform.SetParent(parentTransform, true);
        }
    }




    private bool IsPositionValid(Vector3 position, out List<Collider> blockingColliders)
    {
        blockingColliders = new List<Collider>();
        if (minDistance <= 0f) return true;

        int includedLayers = ~ignoreForMinDistance;
        Collider[] hits = Physics.OverlapSphere(position, minDistance, includedLayers);

        foreach (var c in hits)
        {
            if (!c) continue;

            GameObject root = GetRoot(c.gameObject);
            GameObject sourceAsset = PrefabUtility.GetCorrespondingObjectFromSource(root);

            // Block if this object matches the prefab to paint
            if (sourceAsset == prefabToPaint || avoidPrefabs.Contains(sourceAsset))
            {
                blockingColliders.Add(c);
            }
            // Optional fallback: block objects with the same name
            else if (prefabToPaint != null && root.name.Contains(prefabToPaint.name))
            {
                blockingColliders.Add(c);
            }
        }

        return blockingColliders.Count == 0;
    }


    private GameObject GetRoot(GameObject go)
    {
        if (!go) return null;
        GameObject nearest = PrefabUtility.GetNearestPrefabInstanceRoot(go);
        return nearest != null ? nearest : (go.transform.root ? go.transform.root.gameObject : go);
    }

    private LayerMask LayerMaskField(string label, LayerMask mask)
    {
        string[] layers = new string[32];
        for (int i = 0; i < 32; i++)
            layers[i] = string.IsNullOrEmpty(LayerMask.LayerToName(i)) ? $"Layer {i}" : LayerMask.LayerToName(i);

        int maskValue = EditorGUILayout.MaskField(label, mask.value, layers);
        mask.value = maskValue;
        return mask;
    }
}
#endif