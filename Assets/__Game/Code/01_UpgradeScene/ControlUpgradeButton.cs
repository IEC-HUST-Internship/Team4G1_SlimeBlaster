using UnityEngine;

public class ControlUpgradeButton : MonoBehaviour
{
    [Header("Target to Control")]
    public GameObject targetObject; // The object/map to move
    public GameObject background;   // Background that moves slower (parallax effect)

    [Header("Pan Settings")]
    public float panSpeed = 1f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 3f;

    private Vector3 lastMousePosition;
    private Vector3 dragStartPosition;
    private bool isDragging = false;

    // For mobile pinch zoom
    private float lastPinchDistance = 0f;

    private void Update()
    {
        if (targetObject == null) return;

        // Check all controls every frame
        HandleMouseDrag();
        HandleMouseZoom();
        HandleTouchDrag();
        HandlePinchZoom();
    }

    // PC: Mouse drag to pan
    private void HandleMouseDrag()
    {
        if (PlayerInputHandler.Instance.IsInputDown())
        {
            isDragging = true;
            lastMousePosition = PlayerInputHandler.Instance.GetInputScreenPosition();
        }

        if (PlayerInputHandler.Instance.IsInputActive() && isDragging)
        {
            Vector3 delta = PlayerInputHandler.Instance.GetInputScreenPosition() - lastMousePosition;
            Vector3 movement = new Vector3(delta.x, delta.y, 0) * panSpeed * Time.deltaTime;
            targetObject.transform.position += movement;
            if (background != null)
            {
                background.transform.position += movement * 0.5f;
            }
            lastMousePosition = PlayerInputHandler.Instance.GetInputScreenPosition();
        }

        if (PlayerInputHandler.Instance.IsInputUp())
        {
            isDragging = false;
        }
    }

    // PC: Mouse scroll wheel to zoom
    private void HandleMouseZoom()
    {
        float scroll = PlayerInputHandler.Instance.GetScrollWheel();
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 scale = targetObject.transform.localScale;
            scale += Vector3.one * scroll * zoomSpeed;
            scale.x = Mathf.Clamp(scale.x, minZoom, maxZoom);
            scale.y = Mathf.Clamp(scale.y, minZoom, maxZoom);
            scale.z = Mathf.Clamp(scale.z, minZoom, maxZoom);
            targetObject.transform.localScale = scale;

            if (background != null)
            {
                Vector3 bgScale = background.transform.localScale;
                bgScale += Vector3.one * scroll * zoomSpeed * 0.5f;
                bgScale.x = Mathf.Clamp(bgScale.x, minZoom, maxZoom);
                bgScale.y = Mathf.Clamp(bgScale.y, minZoom, maxZoom);
                bgScale.z = Mathf.Clamp(bgScale.z, minZoom, maxZoom);
                background.transform.localScale = bgScale;
            }
        }
    }

    // Mobile: Touch drag to pan
    private void HandleTouchDrag()
    {
        if (PlayerInputHandler.Instance.GetTouchCount() == 1)
        {
            Touch? touch = PlayerInputHandler.Instance.GetTouch(0);
            if (!touch.HasValue) return;

            if (touch.Value.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastMousePosition = touch.Value.position;
            }
            else if (touch.Value.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 delta = (Vector3)touch.Value.position - lastMousePosition;
                Vector3 movement = new Vector3(delta.x, delta.y, 0) * panSpeed * Time.deltaTime;
                targetObject.transform.position += movement;
                if (background != null)
                {
                    background.transform.position += movement * 0.5f;
                }
                lastMousePosition = touch.Value.position;
            }
            else if (touch.Value.phase == TouchPhase.Ended || touch.Value.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

    // Mobile: Pinch to zoom
    private void HandlePinchZoom()
    {
        if (PlayerInputHandler.Instance.GetTouchCount() == 2)
        {
            Touch? touch1 = PlayerInputHandler.Instance.GetTouch(0);
            Touch? touch2 = PlayerInputHandler.Instance.GetTouch(1);
            
            if (!touch1.HasValue || !touch2.HasValue) return;

            Vector2 touch1PrevPos = touch1.Value.position - touch1.Value.deltaPosition;
            Vector2 touch2PrevPos = touch2.Value.position - touch2.Value.deltaPosition;

            float prevDistance = (touch1PrevPos - touch2PrevPos).magnitude;
            float currentDistance = (touch1.Value.position - touch2.Value.position).magnitude;

            float difference = currentDistance - prevDistance;

            if (Mathf.Abs(difference) > 0.01f)
            {
                Vector3 scale = targetObject.transform.localScale;
                scale += Vector3.one * difference * zoomSpeed * 0.01f;
                scale.x = Mathf.Clamp(scale.x, minZoom, maxZoom);
                scale.y = Mathf.Clamp(scale.y, minZoom, maxZoom);
                scale.z = Mathf.Clamp(scale.z, minZoom, maxZoom);
                targetObject.transform.localScale = scale;

                if (background != null)
                {
                    Vector3 bgScale = background.transform.localScale;
                    bgScale += Vector3.one * difference * zoomSpeed * 0.01f * 0.5f;
                    bgScale.x = Mathf.Clamp(bgScale.x, minZoom, maxZoom);
                    bgScale.y = Mathf.Clamp(bgScale.y, minZoom, maxZoom);
                    bgScale.z = Mathf.Clamp(bgScale.z, minZoom, maxZoom);
                    background.transform.localScale = bgScale;
                }
            }
        }
    }
}
