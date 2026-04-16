using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Zoom (Orthographic)")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float scrollZoomMultiplier = 10f;
    [SerializeField] private float minOrthoSize = 1f;
    [SerializeField] private float maxOrthoSize = 20f;
    [SerializeField] private bool enableScrollZoom = true;
    [SerializeField] private KeyCode zoomInKey = KeyCode.Q;
    [SerializeField] private KeyCode zoomOutKey = KeyCode.E;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f) * moveSpeed * dt;
        transform.position += delta;

        if (enableZoom && cam.orthographic)
        {
            float zoomDelta = 0f;

            if (enableScrollZoom)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                zoomDelta -= scroll * zoomSpeed * scrollZoomMultiplier;
            }

            if (Input.GetKey(zoomInKey))
            {
                zoomDelta -= zoomSpeed * dt;
            }

            if (Input.GetKey(zoomOutKey))
            {
                zoomDelta += zoomSpeed * dt;
            }

            if (!Mathf.Approximately(zoomDelta, 0f))
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + zoomDelta, minOrthoSize, maxOrthoSize);
            }
        }
    }
}
