using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

[EditorTool("Move Group + Snap To Planet")]
public class MoveGroupSnapTool : EditorTool
{
    GUIContent iconContent;

    // Settings
    public float raycastHeight = 100f;
    public LayerMask layerMask = ~0;
    public bool alignToNormal = true;
    public float zOffset = 90f;
    public bool showDebug = true;

    private bool showSettings = true;

    void OnEnable()
    {
        iconContent = new GUIContent("Move+Snap", "Move objects normally, snap all to planet individually");
    }

    public override GUIContent toolbarIcon => iconContent;

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView)) return;

        // --- Draw collapsible settings ---
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 220, 200), GUI.skin.box);
        showSettings = EditorGUILayout.Foldout(showSettings, "Snap Tool Settings");
        if (showSettings)
        {
            raycastHeight = EditorGUILayout.FloatField("Raycast Height", raycastHeight);
            layerMask = LayerMaskField("Layer Mask", layerMask);
            alignToNormal = EditorGUILayout.Toggle("Align to Normal", alignToNormal);
            zOffset = EditorGUILayout.FloatField("Z Rotation", zOffset);
            showDebug = EditorGUILayout.Toggle("Show Debug Rays", showDebug);
        }
        GUILayout.EndArea();
        Handles.EndGUI();

        if (Selection.transforms.Length == 0) return;

        // --- Calculate group center ---
        Vector3 groupCenter = Vector3.zero;
        foreach (Transform t in Selection.transforms)
            groupCenter += t.position;
        groupCenter /= Selection.transforms.Length;

        // --- Draw a single handle for the group ---
        EditorGUI.BeginChangeCheck();
        Vector3 newCenter = Handles.PositionHandle(groupCenter, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Vector3 delta = newCenter - groupCenter;

            foreach (Transform t in Selection.transforms)
            {
                Undo.RecordObject(t, "Move Group + Snap");

                // Apply delta
                Vector3 intendedPosition = t.position + delta;

                // Raycast down from above intended position
                Vector3 start = intendedPosition + Vector3.up * raycastHeight;
                Vector3 direction = Vector3.down;
                float distance = raycastHeight * 2f;

                if (Physics.Raycast(start, direction, out RaycastHit hit, distance, layerMask))
                {
                    t.position = hit.point;

                    if (alignToNormal)
                    {
                        t.up = hit.normal;
                        t.Rotate(Vector3.forward, zOffset, Space.Self);
                    }

                    if (showDebug)
                    {
                        Debug.DrawRay(start, direction * distance, Color.yellow, 2f);
                        Debug.DrawLine(start, hit.point, Color.green, 2f);
                        Debug.DrawRay(hit.point, hit.normal * 2f, Color.cyan, 2f);
                    }
                }
                else
                {
                    t.position = intendedPosition;
                    if (showDebug)
                        Debug.DrawRay(start, direction * distance, Color.red, 2f);
                }
            }
        }
    }

    // Custom LayerMask field
    private LayerMask LayerMaskField(string label, LayerMask mask)
    {
        string[] layers = new string[32];
        for (int i = 0; i < 32; i++)
        {
            layers[i] = LayerMask.LayerToName(i);
            if (string.IsNullOrEmpty(layers[i]))
                layers[i] = "Layer " + i;
        }

        int maskValue = EditorGUILayout.MaskField(label, mask.value, layers);
        mask.value = maskValue;
        return mask;
    }
}
