using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 镜头控制器 - 支持鼠标拖动旋转和滚轮缩放
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("鼠标拖动旋转速度")]
    public float rotationSpeed = 5f;
    
    [Tooltip("垂直旋转角度限制（最小值）")]
    public float minVerticalAngle = -80f;
    
    [Tooltip("垂直旋转角度限制（最大值）")]
    public float maxVerticalAngle = 80f;

    [Header("缩放设置")]
    [Tooltip("滚轮缩放速度")]
    public float zoomSpeed = 10f;
    
    [Tooltip("最小缩放距离")]
    public float minZoom = 2f;
    
    [Tooltip("最大缩放距离")]
    public float maxZoom = 20f;

    [Header("目标设置")]
    [Tooltip("镜头围绕的目标点")]
    public Transform target;
    
    [Tooltip("初始距离")]
    public float initialDistance = 10f;

    private float currentDistance;
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private Vector3 targetPosition;

    void Start()
    {
        // 如果没有指定目标，使用场景中心点
        if (target == null)
        {
            GameObject targetObj = new GameObject("CameraTarget");
            target = targetObj.transform;
            target.position = Vector3.zero;
        }

        targetPosition = target.position;
        currentDistance = initialDistance;

        // 获取初始旋转角度
        Vector3 angles = transform.eulerAngles;
        currentRotationX = angles.y;
        currentRotationY = angles.x;

        // 设置初始位置
        UpdateCameraPosition();
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();
    }

    /// <summary>
    /// 处理鼠标拖动旋转
    /// </summary>
    void HandleRotation()
    {
        // 使用新的Input System检测鼠标按键
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // 按住鼠标右键或中键拖动旋转
        if (mouse.rightButton.isPressed || mouse.middleButton.isPressed)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            float mouseX = mouseDelta.x * rotationSpeed * Time.deltaTime;
            float mouseY = mouseDelta.y * rotationSpeed * Time.deltaTime;

            currentRotationX += mouseX;
            currentRotationY -= mouseY;

            // 限制垂直旋转角度
            currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);
        }
    }

    /// <summary>
    /// 处理滚轮缩放
    /// </summary>
    void HandleZoom()
    {
        // 使用新的Input System检测滚轮
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 scrollDelta = mouse.scroll.ReadValue();
        float scroll = scrollDelta.y / 120f; // 标准化滚轮值
        
        if (scroll != 0f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minZoom, maxZoom);
        }
    }

    /// <summary>
    /// 更新镜头位置
    /// </summary>
    void UpdateCameraPosition()
    {
        // 计算旋转
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        
        // 计算位置（目标点 + 旋转后的偏移）
        Vector3 offset = rotation * new Vector3(0, 0, -currentDistance);
        transform.position = targetPosition + offset;
        
        // 让镜头始终看向目标点
        transform.LookAt(targetPosition);
    }

    /// <summary>
    /// 设置新的目标位置
    /// </summary>
    public void SetTarget(Vector3 newTarget)
    {
        targetPosition = newTarget;
    }

    /// <summary>
    /// 重置镜头到初始状态
    /// </summary>
    public void ResetCamera()
    {
        currentDistance = initialDistance;
        currentRotationX = 0f;
        currentRotationY = 20f;
        UpdateCameraPosition();
    }
}
