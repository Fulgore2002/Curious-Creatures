using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Follow")]
    [Range(0f, 1f)] public float smoothSpeed = 0.125f;

    [Header("World Bounds")]
    public Tilemap tilemap; // assign the Tilemap that defines the playable area

    // computed bounds in world space
    private Vector2 minBounds;
    private Vector2 maxBounds;

    // camera half sizes
    private float camHalfHeight;
    private float camHalfWidth;

    private Camera cam;

    // track screen size to recalc if changed (handles window resize)
    private int lastScreenW, lastScreenH;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;
    }

    void Start()
    {
        RecalculateCameraSize();
        UpdateBounds();
    }

    void LateUpdate()
    {
        // If resolution/aspect changed, recalc sizes & bounds
        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
        {
            RecalculateCameraSize();
            UpdateBounds();
        }

        if (target == null) return;

        Vector3 desired = target.position + offset;

        // If we have bounds, clamp them. If level smaller than view, center on that level.
        if (tilemap != null)
        {
            float minX = minBounds.x + camHalfWidth;
            float maxX = maxBounds.x - camHalfWidth;
            float minY = minBounds.y + camHalfHeight;
            float maxY = maxBounds.y - camHalfHeight;

            // If playable area is smaller than the viewport on X or Y, force min==max so camera centers
            if (minX > maxX)
            {
                // center X on level
                float centerX = (minBounds.x + maxBounds.x) * 0.5f;
                desired.x = centerX;
            }
            else
            {
                desired.x = Mathf.Clamp(desired.x, minX, maxX);
            }

            if (minY > maxY)
            {
                float centerY = (minBounds.y + maxBounds.y) * 0.5f;
                desired.y = centerY;
            }
            else
            {
                desired.y = Mathf.Clamp(desired.y, minY, maxY);
            }
        }

        Vector3 smoothed = Vector3.Lerp(transform.position, new Vector3(desired.x, desired.y, transform.position.z), smoothSpeed);
        transform.position = smoothed;
    }

    // Compute camera half extents using orthographic size and current aspect
    private void RecalculateCameraSize()
    {
        if (cam == null) cam = Camera.main;
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        lastScreenW = Screen.width;
        lastScreenH = Screen.height;
    }

    // Convert Tilemap cell bounds to accurate world-space min/max.
    // This handles tile anchor/size and ensures we include the final tile cell.
    public void UpdateBounds()
    {
        if (tilemap == null)
        {
            minBounds = Vector2.negativeInfinity;
            maxBounds = Vector2.positiveInfinity;
            return;
        }

        // Get the cell bounds 
        BoundsInt b = tilemap.cellBounds;
        Vector3Int minCell = b.min;
        Vector3Int maxCell = b.max; // max is exclusive 

        // Convert to world coordinates. CellToWorld gives bottom-left corner of that cell.
        Vector3 minWorld = tilemap.CellToWorld(minCell);
        Vector3 maxWorld = tilemap.CellToWorld(maxCell);

        
        Vector3 cellSize = tilemap.cellSize;
        maxWorld += cellSize;

        // If the tilemap or grid has transforms, CellToWorld includes them,
        // so minWorld/maxWorld are already in world space.
        minBounds = new Vector2(minWorld.x, minWorld.y);
        maxBounds = new Vector2(maxWorld.x, maxWorld.y);
    }

    // Editor visualization
    void OnDrawGizmosSelected()
    {
        if (tilemap == null) return;

        RecalculateCameraSize();
        UpdateBounds();

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.6f);
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0f);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
        Gizmos.DrawWireCube(center, size);

        // draw clamped camera rect
        Vector3 camCenter = transform.position;
        Gizmos.color = Color.cyan;
        Vector3 camSize = new Vector3(camHalfWidth * 2f, camHalfHeight * 2f, 0f);
        Gizmos.DrawWireCube(camCenter, camSize);
    }
}
