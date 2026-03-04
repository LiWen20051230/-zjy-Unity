using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity Scene视图风格的镜头控制器
/// 右键拖动：旋转视角
/// 中键拖动：平移镜头
/// 滚轮：缩放（前后移动）
/// </summary>
public class SceneViewCameraController : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("鼠标旋转速度")]
    public float rotationSpeed = 3f;
    
    [Tooltip("垂直旋转角度限制（最小值）")]
    public float minVerticalAngle = -89f;
    
    [Tooltip("垂直旋转角度限制（最大值）")]
    public float maxVerticalAngle = 89f;

    [Header("平移设置")]
    [Tooltip("鼠标平移速度")]
    public float panSpeed = 0.5f;

    [Header("缩放设置")]
    [Tooltip("滚轮移动速度")]
    public float zoomSpeed = 10f;
    
    [Tooltip("最小移动速度（距离越近移动越慢）")]
    public float minMoveSpeed = 1f;
    
    [Tooltip("最大移动速度")]
    public float maxMoveSpeed = 50f;

    [Header("平滑设置")]
    [Tooltip("启用平滑移动")]
    public bool enableSmoothing = true;
    
    [Tooltip("平滑系数（越大越平滑）")]
    public float smoothFactor = 5f;

    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        // 获取初始旋转角度
        Vector3 angles = transform.eulerAngles;
        currentRotationX = angles.y;
        currentRotationY = angles.x;

        // 初始化目标位置和旋转
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        HandleRotation();
        HandlePan();
        HandleZoom();
        ApplyTransform();
    }

    /// <summary>
    /// 处理旋转（右键拖动）
    /// </summary>
    void HandleRotation()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // 右键拖动旋转
        if (mouse.rightButton.isPressed)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            float mouseX = mouseDelta.x * rotationSpeed * Time.deltaTime;
            float mouseY = mouseDelta.y * rotationSpeed * Time.deltaTime;

            currentRotationX += mouseX;
            currentRotationY -= mouseY;

            // 限制垂直旋转角度
            currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);

            // 更新目标旋转
            targetRotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        }
    }

    /// <summary>
    /// 处理平移（中键拖动）
    /// </summary>
    void HandlePan()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // 中键拖动平移
        if (mouse.middleButton.isPressed)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            
            // 根据鼠标移动计算平移方向
            Vector3 moveRight = transform.right * (-mouseDelta.x * panSpeed * Time.deltaTime);
            Vector3 moveUp = transform.up * (-mouseDelta.y * panSpeed * Time.deltaTime);
            
            targetPosition += moveRight + moveUp;
        }
    }

    /// <summary>
    /// 处理缩放（滚轮前后移动）
    /// </summary>
    void HandleZoom()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 scrollDelta = mouse.scroll.ReadValue();
        float scroll = scrollDelta.y / 120f; // 标准化滚轮值
        
        if (scroll != 0f)
        {
            // 根据当前速度动态调整移动距离
            float currentSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, Mathf.Abs(scroll));
            Vector3 moveForward = transform.forward * scroll * currentSpeed * Time.deltaTime * 100f;
            
            targetPosition += moveForward;
        }
    }

    /// <summary>
    /// 应用变换（支持平滑）
    /// </summary>
    void ApplyTransform()
    {
        if (enableSmoothing)
        {
            // 平滑插值
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothFactor);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothFactor);
        }
        else
        {
            // 直接应用
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// 重置镜头到初始状态
    /// </summary>
    public void ResetCamera(Vector3 position, Vector3 eulerAngles)
    {
        targetPosition = position;
        currentRotationX = eulerAngles.y;
        currentRotationY = eulerAngles.x;
        targetRotation = Quaternion.Euler(eulerAngles);
        
        if (!enableSmoothing)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// 聚焦到指定位置
    /// </summary>
    public void FocusOn(Vector3 focusPoint, float distance = 10f)
    {
        Vector3 direction = (transform.position - focusPoint).normalized;
        targetPosition = focusPoint + direction * distance;
    }
}
