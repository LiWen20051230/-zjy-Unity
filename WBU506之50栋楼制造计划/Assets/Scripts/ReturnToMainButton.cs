using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 返回主场景按钮 - 点击后返回主场景并带淡入淡出效果
/// </summary>
[RequireComponent(typeof(Button))]
public class ReturnToMainButton : MonoBehaviour
{
    [Header("主场景设置")]
    [Tooltip("主场景名称")]
    public string mainSceneName = "1";
    
    [Tooltip("或使用主场景索引（-1表示使用场景名称）")]
    public int mainSceneIndex = -1;

    [Header("调试")]
    [Tooltip("启用调试日志")]
    public bool enableDebugLog = true;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        
        if (button == null)
        {
            Debug.LogError($"{gameObject.name}: 没有找到Button组件！");
            return;
        }

        // 添加点击事件监听
        button.onClick.AddListener(OnButtonClick);
        
        if (enableDebugLog)
        {
            Debug.Log($"{gameObject.name}: ReturnToMainButton已初始化");
            Debug.Log($"  - Button Interactable: {button.interactable}");
            Debug.Log($"  - 目标场景: {(mainSceneIndex >= 0 ? $"索引 {mainSceneIndex}" : mainSceneName)}");
        }

        // 检查必要组件
        CheckRequiredComponents();
    }

    void CheckRequiredComponents()
    {
        // 检查EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogError("场景中没有EventSystem！按钮无法响应点击。请添加：GameObject → UI → Event System");
        }

        // 检查Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"{gameObject.name}: Button不在Canvas下！");
        }
        else
        {
            // 检查GraphicRaycaster
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                Debug.LogError($"{canvas.gameObject.name}: Canvas上没有GraphicRaycaster组件！");
            }
        }

        // 检查Image的Raycast Target
        Image image = GetComponent<Image>();
        if (image != null && !image.raycastTarget)
        {
            Debug.LogWarning($"{gameObject.name}: Image的Raycast Target被禁用，按钮可能无法点击！");
        }
    }

    /// <summary>
    /// 按钮点击事件
    /// </summary>
    void OnButtonClick()
    {
        if (enableDebugLog)
        {
            Debug.Log($"{gameObject.name}: 按钮被点击！准备返回主场景...");
        }

        // 检查SceneTransitionManager是否存在
        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("场景中没有SceneTransitionManager！请确保主场景中有该组件。");
            return;
        }

        // 返回主场景
        if (mainSceneIndex >= 0)
        {
            if (enableDebugLog)
            {
                Debug.Log($"使用场景索引切换: {mainSceneIndex}");
            }
            SceneTransitionManager.Instance.TransitionToScene(mainSceneIndex);
        }
        else if (!string.IsNullOrEmpty(mainSceneName))
        {
            if (enableDebugLog)
            {
                Debug.Log($"使用场景名称切换: {mainSceneName}");
            }
            SceneTransitionManager.Instance.TransitionToScene(mainSceneName);
        }
        else
        {
            Debug.LogWarning("没有设置主场景名称或索引！");
        }
    }

    void OnDestroy()
    {
        // 移除监听器
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    // 用于测试的公共方法
    public void TestClick()
    {
        Debug.Log($"{gameObject.name}: TestClick被调用");
        OnButtonClick();
    }
}
