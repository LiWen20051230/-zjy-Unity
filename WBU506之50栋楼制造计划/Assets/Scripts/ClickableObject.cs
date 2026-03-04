using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 可点击物体 - 点击后触发场景切换
/// </summary>
[RequireComponent(typeof(Collider))]
public class ClickableObject : MonoBehaviour
{
    [Header("场景设置")]
    [Tooltip("目标场景名称")]
    public string targetSceneName;
    
    [Tooltip("或使用场景索引（-1表示使用场景名称）")]
    public int targetSceneIndex = -1;

    [Header("视觉反馈")]
    [Tooltip("鼠标悬停时的高亮颜色")]
    public Color hoverColor = new Color(1f, 1f, 0.5f, 1f);
    
    [Tooltip("是否启用高亮效果")]
    public bool enableHighlight = true;

    private Renderer objectRenderer;
    private Color originalColor;
    private Material materialInstance;
    private bool isHovering = false;

    void Start()
    {
        // 获取Renderer组件
        objectRenderer = GetComponent<Renderer>();
        
        if (objectRenderer != null && enableHighlight)
        {
            // 创建材质实例以避免影响其他物体
            materialInstance = objectRenderer.material;
            originalColor = materialInstance.color;
        }

        // 确保有Collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"{gameObject.name} 没有Collider组件，无法检测点击！");
        }
    }

    void Update()
    {
        CheckMouseClick();
    }

    /// <summary>
    /// 检测鼠标点击
    /// </summary>
    void CheckMouseClick()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // 检测鼠标左键点击
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    OnClicked();
                }
            }
        }

        // 检测鼠标悬停（用于高亮效果）
        if (enableHighlight && objectRenderer != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (!isHovering)
                    {
                        OnHoverEnter();
                    }
                }
                else
                {
                    if (isHovering)
                    {
                        OnHoverExit();
                    }
                }
            }
            else
            {
                if (isHovering)
                {
                    OnHoverExit();
                }
            }
        }
    }

    /// <summary>
    /// 点击时触发
    /// </summary>
    void OnClicked()
    {
        Debug.Log($"点击了 {gameObject.name}，准备切换场景...");

        // 检查SceneTransitionManager是否存在
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("场景中没有SceneTransitionManager！请添加该组件。");
            return;
        }

        // 切换场景
        if (targetSceneIndex >= 0)
        {
            SceneTransitionManager.Instance.TransitionToScene(targetSceneIndex);
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 没有设置目标场景！");
        }
    }

    /// <summary>
    /// 鼠标进入时
    /// </summary>
    void OnHoverEnter()
    {
        isHovering = true;
        if (materialInstance != null)
        {
            materialInstance.color = hoverColor;
        }
    }

    /// <summary>
    /// 鼠标离开时
    /// </summary>
    void OnHoverExit()
    {
        isHovering = false;
        if (materialInstance != null)
        {
            materialInstance.color = originalColor;
        }
    }

    void OnDestroy()
    {
        // 清理材质实例
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}
