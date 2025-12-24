using UnityEngine;

/// <summary>
/// 🎮 Singleton that handles all player input (mouse/touch)
/// Provides world position of input with optional offset
/// </summary>
public class PlayerInputHandler : Singleton<PlayerInputHandler>
{
    private Camera mainCamera;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    /// <summary>
    /// 🎯 Check if player is currently providing input (mouse button held or touch active)
    /// </summary>
    public bool IsInputActive()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButton(0);
#elif UNITY_IOS || UNITY_ANDROID
        return Input.touchCount > 0;
#else
        return false;
#endif
    }

    /// <summary>
    /// 👆 Check if player just started input this frame (mouse button down or touch began)
    /// </summary>
    public bool IsInputDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButtonDown(0);
#elif UNITY_IOS || UNITY_ANDROID
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#else
        return false;
#endif
    }

    /// <summary>
    /// 📍 Get raw screen position of input (for UI raycasts)
    /// </summary>
    public Vector3 GetInputScreenPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.mousePosition;
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        return Vector3.zero;
#else
        return Vector3.zero;
#endif
    }

    public Vector3 GetInputWorldPosition()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 inputScreenPos = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        // 🖱️ PC: get mouse position
        inputScreenPos = Input.mousePosition;
#elif UNITY_IOS || UNITY_ANDROID
        // 📱 Mobile: get first touch position
        if (Input.touchCount > 0)
        {
            inputScreenPos = Input.GetTouch(0).position;
        }
#endif

        inputScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(inputScreenPos);
        worldPos.z = 0f; // 📏 Keep on Z=0 plane

        return worldPos;
    }

    /// <summary>
    /// 👆 Check if player just released input this frame (mouse button up or touch ended)
    /// </summary>
    public bool IsInputUp()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButtonUp(0);
#elif UNITY_IOS || UNITY_ANDROID
        return Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled);
#else
        return false;
#endif
    }

    /// <summary>
    /// 🔢 Get the number of active touches (0 on PC if mouse not pressed)
    /// </summary>
    public int GetTouchCount()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButton(0) ? 1 : 0;
#elif UNITY_IOS || UNITY_ANDROID
        return Input.touchCount;
#else
        return 0;
#endif
    }

    /// <summary>
    /// 📱 Get touch by index (for multi-touch like pinch zoom)
    /// </summary>
    public Touch? GetTouch(int index)
    {
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > index)
        {
            return Input.GetTouch(index);
        }
#endif
        return null;
    }

    /// <summary>
    /// 🔍 Get mouse scroll wheel value (PC only, returns 0 on mobile)
    /// </summary>
    public float GetScrollWheel()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetAxis("Mouse ScrollWheel");
#else
        return 0f;
#endif
    }

    /// <summary>
    /// 📱 Check if touch is in a specific phase
    /// </summary>
    public bool IsTouchPhase(int touchIndex, TouchPhase phase)
    {
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > touchIndex)
        {
            return Input.GetTouch(touchIndex).phase == phase;
        }
#endif
        return false;
    }

    /// <summary>
    /// 📱 Get touch delta position (movement since last frame)
    /// </summary>
    public Vector2 GetTouchDeltaPosition(int touchIndex)
    {
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > touchIndex)
        {
            return Input.GetTouch(touchIndex).deltaPosition;
        }
#endif
        return Vector2.zero;
    }

    /// <summary>
    /// 📱 Get touch position by index
    /// </summary>
    public Vector2 GetTouchPosition(int touchIndex)
    {
#if UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > touchIndex)
        {
            return Input.GetTouch(touchIndex).position;
        }
#endif
        return Vector2.zero;
    }
}
