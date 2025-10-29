using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Smooth Follow")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f;

    [Header("World Bounds")]
    public Tilemap tilemap; 

    private Vector2 minBounds;
    private Vector2 maxBounds;
    private float camHalfHeight;
    private float camHalfWidth;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        if (tilemap != null)
        {
            // Use world bounds
            Bounds tilemapBounds = tilemap.localBounds;
            Vector3 tilemapWorldPos = tilemap.transform.position;
            minBounds = tilemapWorldPos + tilemapBounds.min;
            maxBounds = tilemapWorldPos + tilemapBounds.max;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        // Clamp camera within bounds 
        if (tilemap != null)
        {
            float clampedX = Mathf.Clamp(desiredPosition.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
            float clampedY = Mathf.Clamp(desiredPosition.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);
            desiredPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
