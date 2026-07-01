using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GrassFieldGenerator : MonoBehaviour
{
    [Header("Grass Settings")]
    [Tooltip("The grass blade prefab to duplicate.")]
    public GameObject grassPrefab;

    [Tooltip("How many grass blades along the X axis.")]
    public int countX = 20;

    [Tooltip("How many grass blades along the Z axis.")]
    public int countZ = 20;

    [Tooltip("Spacing between grass blades.")]
    public float spacing = 0.5f;

    [Tooltip("Randomize the rotation of each grass blade.")]
    public bool randomizeRotation = true;

    [Header("Plane Settings")]
    [Tooltip("The plane to place the grass on.")]
    public Transform groundPlane;

    [Tooltip("Where to start placing grass on the X/Z plane. 0 = corner, 0.5 = center, 1 = opposite corner.")]
    [Range(0f, 1f)]
    public float anchorPosition = 0f;

    [Header("Scale Randomization")]
    [Tooltip("Randomize the scale of each grass blade.")]
    public bool randomizeScale = false;

    [Tooltip("Minimum height scale (0.5 = half height, 1.5 = 1.5x height).")]
    [Range(0.5f, 2f)]
    public float minHeightScale = 0.8f;

    [Tooltip("Maximum height scale (0.5 = half height, 1.5 = 1.5x height).")]
    [Range(0.5f, 2f)]
    public float maxHeightScale = 1.2f;

    [Tooltip("Minimum width/Depth scale (0.5 = half width, 1.5 = 1.5x width).")]
    [Range(0.5f, 1.5f)]
    public float minWidthScale = 0.9f;

    [Tooltip("Maximum width/Depth scale (0.5 = half width, 1.5 = 1.5x width).")]
    [Range(0.5f, 1.5f)]
    public float maxWidthScale = 1.1f;

    [Header("Position Randomization")]
    [Tooltip("Add random offset to position for natural look.")]
    public bool randomizePosition = true;

    [Tooltip("How much random offset to apply.")]
    [Range(0f, 0.5f)]
    public float randomOffset = 0.15f;

    [ContextMenu("Generate Grass Field")]
    public void GenerateGrassField()
    {
        GameObject existing = GameObject.Find("GrassField");
        if (existing != null)
        {
            DestroyImmediate(existing);
        }

        if (grassPrefab == null)
        {
            Debug.LogError("GrassFieldGenerator: Grass Prefab is not assigned.", this);
            return;
        }

        if (groundPlane == null)
        {
            Debug.LogError("GrassFieldGenerator: Ground Plane is not assigned.", this);
            return;
        }

        Renderer planeRenderer = groundPlane.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogError("GrassFieldGenerator: Ground Plane has no Renderer. Assign a plane with a MeshRenderer.", this);
            return;
        }

        Vector3 planeSize = planeRenderer.bounds.size;
        float planeWidth = planeSize.x;
        float planeDepth = planeSize.z;

        // Grass is placed at the plane's Y position - since the plane's pivot is centered, adjust
        // the generated GrassField GameObject's Y position afterward to fine-tune height.
        Vector3 groundPosition = groundPlane.position;
        float verticalPos = groundPosition.y;

        float halfWidth = planeWidth / 2f;
        float halfDepth = planeDepth / 2f;

        Vector3 corner = new Vector3(
            groundPosition.x - halfWidth,
            verticalPos,
            groundPosition.z - halfDepth
        );

        float anchorX = anchorPosition * planeWidth;
        float anchorZ = anchorPosition * planeDepth;

        Vector3 startPos = new Vector3(
            corner.x + anchorX,
            verticalPos,
            corner.z + anchorZ
        );

        GameObject grassParent = new GameObject("GrassField");
        grassParent.transform.position = Vector3.zero;

        int totalGrass = 0;

        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                Vector3 pos = new Vector3(
                    startPos.x + x * spacing + (randomizePosition ? Random.Range(-randomOffset, randomOffset) : 0f),
                    startPos.y,
                    startPos.z + z * spacing + (randomizePosition ? Random.Range(-randomOffset, randomOffset) : 0f)
                );

                GameObject grass = Instantiate(grassPrefab, pos, Quaternion.identity, grassParent.transform);
                totalGrass++;

                if (randomizeRotation)
                {
                    grass.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
                }

                if (randomizeScale)
                {
                    Vector3 originalScale = grass.transform.localScale;
                    float heightScale = Random.Range(minHeightScale, maxHeightScale);
                    float widthScale = Random.Range(minWidthScale, maxWidthScale);
                    grass.transform.localScale = new Vector3(
                        originalScale.x * widthScale,
                        originalScale.y * heightScale,
                        originalScale.z * widthScale
                    );
                }
            }
        }

        Debug.Log($"GrassFieldGenerator: Generated {totalGrass} grass blades.", this);
    }

    [ContextMenu("Reset Generator")]
    public void ResetGenerator()
    {
        GameObject existing = GameObject.Find("GrassField");
        if (existing != null)
        {
            DestroyImmediate(existing);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (grassPrefab == null || groundPlane == null) return;

        Renderer planeRenderer = groundPlane.GetComponent<Renderer>();
        if (planeRenderer == null) return;

        Vector3 planeSize = planeRenderer.bounds.size;
        float planeWidth = planeSize.x;
        float planeDepth = planeSize.z;

        Vector3 groundPosition = groundPlane.position;

        float halfWidth = planeWidth / 2f;
        float halfDepth = planeDepth / 2f;

        Vector3 corner = new Vector3(
            groundPosition.x - halfWidth,
            groundPosition.y,
            groundPosition.z - halfDepth
        );

        float anchorX = anchorPosition * planeWidth;
        float anchorZ = anchorPosition * planeDepth;

        Vector3 startPos = new Vector3(
            corner.x + anchorX,
            groundPosition.y,
            corner.z + anchorZ
        );

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                Vector3 pos = new Vector3(
                    startPos.x + x * spacing,
                    startPos.y,
                    startPos.z + z * spacing
                );
                Gizmos.DrawSphere(pos, 0.05f);
            }
        }

        Vector3 center = new Vector3(groundPosition.x, groundPosition.y, groundPosition.z);
        Vector3 size = new Vector3(planeWidth, 0.1f, planeDepth);
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startPos, 0.2f);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        countX = Mathf.Max(1, countX);
        countZ = Mathf.Max(1, countZ);
        spacing = Mathf.Max(0.01f, spacing);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(GrassFieldGenerator))]
public class GrassFieldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        GrassFieldGenerator generator = (GrassFieldGenerator)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate Grass", GUILayout.Height(30)))
        {
            generator.GenerateGrassField();
        }

        if (GUILayout.Button("Reset", GUILayout.Height(30)))
        {
            generator.ResetGenerator();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Generate: Creates the grass field at the plane's position.\n" +
            "Reset: Deletes the grass field.\n\n" +
            "Tip: After generating, adjust the GrassField GameObject's Y position to fine-tune height.",
            MessageType.Info
        );
    }
}
#endif
