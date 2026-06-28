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

    // Debug tracking
    private int lastChildCount = 0;

    void Start()
    {
        Debug.Log("GrassFieldGenerator.Start() - Skipping regeneration");
    }

    void Update()
    {
        GameObject grassFieldObj = GameObject.Find("GrassField");
        if (grassFieldObj != null)
        {
            Transform grassField = grassFieldObj.transform;
            if (grassField.childCount != lastChildCount)
            {
                Debug.Log($"🌿 GrassField child count changed: {lastChildCount} → {grassField.childCount}");
                lastChildCount = grassField.childCount;
                
                if (grassField.childCount == 1)
                {
                    Transform remaining = grassField.GetChild(0);
                    Debug.Log($"   Remaining: {remaining.name}, Active: {remaining.gameObject.activeSelf}");
                }
            }
        }
        else
        {
            if (lastChildCount != 0)
            {
                Debug.Log($"💀 GrassField was destroyed! Last count: {lastChildCount}");
                lastChildCount = 0;
            }
        }
    }

    [ContextMenu("Generate Grass Field")]
    public void GenerateGrassField()
    {
        GameObject existing = GameObject.Find("GrassField");
        if (existing != null)
        {
            Debug.Log("🗑️ Deleting existing GrassField...");
            DestroyImmediate(existing);
        }

        if (grassPrefab == null)
        {
            Debug.LogError("❌ GrassPrefab is not assigned!");
            return;
        }

        if (groundPlane == null)
        {
            Debug.LogError("❌ GroundPlane is not assigned!");
            return;
        }

        Debug.Log("=== STARTING GRASS GENERATION ===");
        Debug.Log($"📐 Grid: {countX}x{countZ}, Spacing: {spacing}");

        // --- Get the plane's actual size from its renderer ---
        Renderer planeRenderer = groundPlane.GetComponent<Renderer>();
        if (planeRenderer == null)
        {
            Debug.LogError("❌ GroundPlane has no Renderer! Please assign a plane with a MeshRenderer.");
            return;
        }

        Vector3 planeSize = planeRenderer.bounds.size;
        float planeWidth = planeSize.x;
        float planeDepth = planeSize.z;

        Debug.Log($"📐 Plane size: {planeWidth} x {planeDepth}");

        Vector3 groundPosition = groundPlane.position;
        
        // --- Place grass at the BOTTOM of the plane ---
        // Since the plane's pivot is at its center, the bottom is at groundPosition.y - (planeHeight/2)
        // But we'll just use groundPosition.y and let the user adjust the GrassField Y position
        float verticalPos = groundPosition.y;

        // --- Calculate the corner of the plane (X/Z) ---
        float halfWidth = planeWidth / 2f;
        float halfDepth = planeDepth / 2f;
        
        Vector3 corner = new Vector3(
            groundPosition.x - halfWidth,
            verticalPos,
            groundPosition.z - halfDepth
        );
        
        // Apply anchor offset (0 = corner, 0.5 = center, 1 = opposite corner)
        float anchorX = anchorPosition * planeWidth;
        float anchorZ = anchorPosition * planeDepth;
        
        Vector3 startPos = new Vector3(
            corner.x + anchorX,
            verticalPos,
            corner.z + anchorZ
        );

        Debug.Log($"📍 Plane center: {groundPosition}");
        Debug.Log($"📍 Start position: {startPos}");

        // Create parent at root level
        GameObject grassParent = new GameObject("GrassField");
        grassParent.transform.position = Vector3.zero;

        int totalGrass = 0;
        int totalAttempts = countX * countZ;

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

        lastChildCount = totalGrass;
        Debug.Log($"✅ Generated {totalGrass} grass blades out of {totalAttempts} attempts!");
        Debug.Log($"🌿 GrassField has {grassParent.transform.childCount} children.");
        Debug.Log("💡 Tip: Adjust the GrassField GameObject's Y position to fine-tune grass height.");
    }

    [ContextMenu("Reset Generator")]
    public void ResetGenerator()
    {
        GameObject existing = GameObject.Find("GrassField");
        if (existing != null)
        {
            Debug.Log("🗑️ Deleting GrassField...");
            DestroyImmediate(existing);
        }
        lastChildCount = 0;
        Debug.Log("Generator reset. You can now regenerate.");
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
        
        Vector3 center = new Vector3(
            groundPosition.x,
            groundPosition.y,
            groundPosition.z
        );
        Vector3 size = new Vector3(
            planeWidth,
            0.1f,
            planeDepth
        );
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawWireCube(center, size);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(startPos, 0.2f);
    }

    #if UNITY_EDITOR
    void OnValidate()
    {
        // Ensure values are valid
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
        
        if (GUILayout.Button("🌿 Generate Grass", GUILayout.Height(30)))
        {
            generator.GenerateGrassField();
        }
        
        if (GUILayout.Button("🗑️ Reset", GUILayout.Height(30)))
        {
            generator.ResetGenerator();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "🌿 Generate: Creates the grass field at the plane's Y position.\n" +
            "🗑️ Reset: Deletes the grass field.\n\n" +
            "💡 Tip: After generating, manually adjust the GrassField GameObject's Y position to fine-tune height.",
            MessageType.Info
        );
    }
}
#endif