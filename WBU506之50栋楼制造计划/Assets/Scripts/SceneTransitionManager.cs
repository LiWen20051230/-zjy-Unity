using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 镜头移动行为枚举
/// </summary>
public enum CameraMoveBehavior
{
    MoveBack,      // 向后退
    StayInPlace,   // 保持原位
    OnlyRotate     // 只旋转不移动
}

/// <summary>
/// 场景过渡管理器 - 处理淡入淡出效果和场景切换
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    [Header("过渡设置")]
    [Tooltip("淡入淡出持续时间（秒）")]
    public float fadeDuration = 1f;
    
    [Tooltip("淡出颜色")]
    public Color fadeColor = Color.black;

    [Header("镜头移动设置")]
    [Tooltip("镜头移动持续时间")]
    public float cameraMoveTime = 1.5f;
    
    [Tooltip("镜头移动到目标的距离偏移")]
    public float cameraDistanceOffset = 3f;
    
    [Tooltip("最小安全距离（防止穿模）")]
    public float minSafeDistance = 2f;
    
    [Tooltip("当距离太近时的行为")]
    public CameraMoveBehavior nearBehavior = CameraMoveBehavior.MoveBack;

    private Canvas fadeCanvas;
    private Image fadeImage;
    private static SceneTransitionManager instance;

    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneTransitionManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeFadeCanvas();
    }

    /// <summary>
    /// 初始化淡入淡出画布
    /// </summary>
    void InitializeFadeCanvas()
    {
        // 创建Canvas（作为根物体，不设置父物体）
        GameObject canvasObj = new GameObject("FadeCanvas");
        
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // 确保在最上层
        
        // 添加CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // 添加GraphicRaycaster
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建黑色遮罩Image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = fadeColor;
        
        // 设置为全屏
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // 初始设置为透明（不可见）
        SetFadeAlpha(0f);
        
        // Canvas必须是根物体才能使用DontDestroyOnLoad
        DontDestroyOnLoad(canvasObj);
    }

    /// <summary>
    /// 带淡入淡出效果的场景切换
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName, null));
    }

    /// <summary>
    /// 带淡入淡出效果的场景切换（使用场景索引）
    /// </summary>
    public void TransitionToScene(int sceneIndex)
    {
        StartCoroutine(TransitionCoroutine(sceneIndex, null));
    }

    /// <summary>
    /// 带镜头移动和淡入淡出效果的场景切换
    /// </summary>
    public void TransitionToSceneWithCamera(string sceneName, Vector3 targetPosition)
    {
        StartCoroutine(TransitionCoroutine(sceneName, targetPosition));
    }

    /// <summary>
    /// 带镜头移动和淡入淡出效果的场景切换（使用场景索引）
    /// </summary>
    public void TransitionToSceneWithCamera(int sceneIndex, Vector3 targetPosition)
    {
        StartCoroutine(TransitionCoroutine(sceneIndex, targetPosition));
    }

    private IEnumerator TransitionCoroutine(string sceneName, Vector3? targetPosition)
    {
        // 如果提供了目标位置，先移动镜头
        if (targetPosition.HasValue)
        {
            yield return StartCoroutine(MoveCameraToTarget(targetPosition.Value));
        }

        // 淡出到黑色
        yield return StartCoroutine(FadeOut());
        
        // 加载新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 等待场景加载完成
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        
        // 等待场景激活
        yield return new WaitForSeconds(0.1f);

        // 从黑色淡入
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator TransitionCoroutine(int sceneIndex, Vector3? targetPosition)
    {
        // 如果提供了目标位置，先移动镜头
        if (targetPosition.HasValue)
        {
            yield return StartCoroutine(MoveCameraToTarget(targetPosition.Value));
        }

        // 淡出到黑色
        yield return StartCoroutine(FadeOut());
        
        // 加载新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        // 等待场景加载完成
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        
        // 等待场景激活
        yield return new WaitForSeconds(0.1f);

        // 从黑色淡入
        yield return StartCoroutine(FadeIn());
    }

    /// <summary>
    /// 移动镜头到目标位置
    /// </summary>
    private IEnumerator MoveCameraToTarget(Vector3 targetPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("找不到主摄像机，跳过镜头移动");
            yield break;
        }

        // 禁用镜头控制器（如果有）
        SceneViewCameraController cameraController = mainCamera.GetComponent<SceneViewCameraController>();
        bool wasControllerEnabled = false;
        if (cameraController != null)
        {
            wasControllerEnabled = cameraController.enabled;
            cameraController.enabled = false;
        }

        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 计算到目标的距离和方向
        Vector3 directionToTarget = (targetPosition - startPosition).normalized;
        float distanceToTarget = Vector3.Distance(startPosition, targetPosition);

        // 智能计算最终位置
        Vector3 finalPosition;
        bool shouldMove = true;
        
        if (distanceToTarget <= minSafeDistance)
        {
            // 距离太近，根据设置的行为处理
            switch (nearBehavior)
            {
                case CameraMoveBehavior.MoveBack:
                    // 向后退到安全距离
                    finalPosition = startPosition - mainCamera.transform.forward * (minSafeDistance - distanceToTarget + 1f);
                    Debug.Log($"镜头距离目标太近({distanceToTarget:F2}米)，向后退到安全距离");
                    break;
                    
                case CameraMoveBehavior.StayInPlace:
                    // 保持原位不动
                    finalPosition = startPosition;
                    Debug.Log($"镜头距离目标太近({distanceToTarget:F2}米)，保持当前位置");
                    break;
                    
                case CameraMoveBehavior.OnlyRotate:
                    // 只旋转不移动
                    finalPosition = startPosition;
                    shouldMove = false;
                    Debug.Log($"镜头距离目标太近({distanceToTarget:F2}米)，只旋转朝向目标");
                    break;
                    
                default:
                    finalPosition = startPosition;
                    break;
            }
        }
        else if (distanceToTarget <= cameraDistanceOffset * 1.5f)
        {
            // 距离适中，保持当前位置
            finalPosition = startPosition;
            Debug.Log($"镜头距离目标适中({distanceToTarget:F2}米)，保持当前位置");
        }
        else
        {
            // 距离较远，正常靠近
            finalPosition = targetPosition - directionToTarget * cameraDistanceOffset;
            Debug.Log($"镜头距离目标较远({distanceToTarget:F2}米)，向目标靠近");
        }

        // 计算目标旋转（看向目标点）
        Vector3 lookDirection = targetPosition - finalPosition;
        Quaternion finalRotation = lookDirection != Vector3.zero 
            ? Quaternion.LookRotation(lookDirection) 
            : startRotation;

        float elapsed = 0f;
        float moveAndFadeDuration = cameraMoveTime;

        while (elapsed < moveAndFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveAndFadeDuration;
            
            // 使用平滑曲线
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // 移动镜头（如果需要）
            if (shouldMove)
            {
                mainCamera.transform.position = Vector3.Lerp(startPosition, finalPosition, smoothT);
            }
            
            // 旋转镜头
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, finalRotation, smoothT);
            
            // 同时开始淡出（在移动的后半段）
            if (t > 0.3f)
            {
                float fadeT = (t - 0.3f) / 0.7f;
                SetFadeAlpha(fadeT);
            }

            yield return null;
        }

        // 确保最终位置准确
        if (shouldMove)
        {
            mainCamera.transform.position = finalPosition;
        }
        mainCamera.transform.rotation = finalRotation;
        SetFadeAlpha(1f);
    }

    /// <summary>
    /// 淡出效果（渐渐变黑）
    /// </summary>
    public IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(1f);
    }

    /// <summary>
    /// 淡入效果（从黑色渐渐变亮）
    /// </summary>
    public IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(0f);
    }

    /// <summary>
    /// 设置遮罩透明度
    /// </summary>
    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
}
