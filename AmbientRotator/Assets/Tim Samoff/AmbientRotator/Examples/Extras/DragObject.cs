using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DragObject : MonoBehaviour
{
    [Header("Drag Settings")]
    public bool enableInPlayMode = true;
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool hasRenderer = false;
    private float zDepth;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }
        
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"DragObject on {gameObject.name} has no Collider!");
            return;
        }
        
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            hasRenderer = true;
            originalColor = objectRenderer.material.color;
        }
    }
    
    void Update()
    {
        if (!enableInPlayMode) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                    zDepth = mainCamera.WorldToScreenPoint(transform.position).z;
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = zDepth;
                    offset = transform.position - mainCamera.ScreenToWorldPoint(mousePos);
                    
                    if (hasRenderer)
                    {
                        objectRenderer.material.color = selectedColor;
                    }
                    Debug.Log($"🖱️ Started dragging: {gameObject.name}");
                }
            }
        }
        
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = zDepth;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            transform.position = worldPos + offset;
        }
        
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (hasRenderer)
            {
                objectRenderer.material.color = originalColor;
            }
            Debug.Log($"🖱️ Stopped dragging: {gameObject.name}");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}