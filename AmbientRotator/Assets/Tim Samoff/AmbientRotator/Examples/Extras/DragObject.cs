using UnityEngine;
using UnityEngine.InputSystem;

public class DraggObject : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("Enable to drag in Play Mode.")]
    public bool enableInPlayMode = true;
    
    [Tooltip("Color when selected.")]
    public Color selectedColor = Color.yellow;
    
    [Tooltip("Color when not selected.")]
    public Color defaultColor = Color.white;
    
    [Header("Gizmo Settings")]
    [Tooltip("Show a selection ring in the Scene view.")]
    public bool showGizmo = true;
    
    [Tooltip("Color of the selection ring.")]
    public Color gizmoColor = new Color(0f, 0.8f, 1f, 0.5f);
    
    [Tooltip("Radius of the selection ring.")]
    public float gizmoRadius = 0.6f;
    
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;
    private float zDepth;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool hasRenderer = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        objectRenderer = GetComponent<Renderer>();
        
        if (objectRenderer != null)
        {
            hasRenderer = true;
            originalColor = objectRenderer.material.color;
        }
        
        // Ensure there's a collider for raycasting
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"Draggable3D on {gameObject.name} has no Collider! Adding one...");
            gameObject.AddComponent<SphereCollider>();
        }
    }

    private void Update()
    {
        if (!enableInPlayMode) return;
        
        // Get pointer availability from the New Input System
        Mouse mouse = Mouse.current;
        Touchscreen touch = Touchscreen.current;

        Vector2 screenPosition = Vector2.zero;
        bool isPressed = false;
        bool pressedThisFrame = false;

        // Fallback checks to support both Mouse and Mobile Touch inputs seamlessly
        if (mouse != null)
        {
            screenPosition = mouse.position.ReadValue();
            isPressed = mouse.leftButton.isPressed;
            pressedThisFrame = mouse.leftButton.wasPressedThisFrame;
        }
        else if (touch != null && touch.touches.Count > 0)
        {
            screenPosition = touch.touches[0].position.ReadValue();
            isPressed = touch.touches[0].press.isPressed;
            pressedThisFrame = touch.touches[0].press.wasPressedThisFrame;
        }

        // 1. Detect click/touch start on the object
        if (pressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                    zDepth = mainCamera.WorldToScreenPoint(transform.position).z;
                    offset = transform.position - GetWorldPosition(screenPosition);
                    
                    // Change color when selected
                    if (hasRenderer)
                    {
                        objectRenderer.material.color = selectedColor;
                    }
                    Debug.Log($"🖱️ Started dragging: {gameObject.name}");
                }
            }
        }

        // 2. Drag the object
        if (isDragging && isPressed)
        {
            transform.position = GetWorldPosition(screenPosition) + offset;
        }

        // 3. Stop dragging
        if (!isPressed && isDragging)
        {
            isDragging = false;
            
            // Reset color when released
            if (hasRenderer)
            {
                objectRenderer.material.color = originalColor;
            }
            Debug.Log($"🖱️ Stopped dragging: {gameObject.name}");
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Vector3 screenCoord = new Vector3(screenPos.x, screenPos.y, zDepth);
        return mainCamera.ScreenToWorldPoint(screenCoord);
    }

    // --- Selection Gizmo ---
    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        
        // Draw a selection ring
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        
        // Draw a subtle ring on the ground
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0, transform.position.z), gizmoRadius);
    }

    // --- Editor Gizmo (always visible) ---
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        // Draw a small dot at the object's position
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}