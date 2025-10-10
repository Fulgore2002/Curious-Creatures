using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraBounds : MonoBehaviour
{
    public Transform target;
    public Tilemap tilemap; // Reference to your level's Tilemap
    public float smoothSpeed = 0.125f;

    private Vector2 minBounds;
    private Vector2 maxBounds;
    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        Camera cam = Camera.main;
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        // Get bounds from tilemap
        Bounds tilemapBounds = tilemap.localBounds;
        minBounds = tilemapBounds.min;
        maxBounds = tilemapBounds.max;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position;
        float clampedX = Mathf.Clamp(desiredPosition.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
        float clampedY = Mathf.Clamp(desiredPosition.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, transform.position.z), smoothSpeed);
        transform.position = smoothedPosition;
    }
}