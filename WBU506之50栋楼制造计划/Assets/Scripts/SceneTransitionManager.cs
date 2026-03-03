using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

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
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    /// <summary>
    /// 带淡入淡出效果的场景切换（使用场景索引）
    /// </summary>
    public void TransitionToScene(int sceneIndex)
    {
        StartCoroutine(TransitionCoroutine(sceneIndex));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
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

    private IEnumerator TransitionCoroutine(int sceneIndex)
    {
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
