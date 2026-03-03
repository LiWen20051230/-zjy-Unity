using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// 场景过渡管理器 - 处理模糊效果和场景切换
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    [Header("过渡设置")]
    [Tooltip("模糊过渡持续时间")]
    public float transitionDuration = 1.5f;
    
    [Tooltip("最大模糊强度")]
    public float maxBlurIntensity = 5f;

    [Header("Volume设置")]
    [Tooltip("全局Volume组件（用于后处理效果）")]
    public Volume globalVolume;

    private DepthOfField depthOfField;
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

        InitializeVolume();
    }

    /// <summary>
    /// 初始化Volume和景深效果
    /// </summary>
    void InitializeVolume()
    {
        // 如果没有指定Volume，尝试查找
        if (globalVolume == null)
        {
            globalVolume = FindObjectOfType<Volume>();
        }

        // 如果还是没有，创建一个
        if (globalVolume == null)
        {
            GameObject volumeObj = new GameObject("Global Volume");
            globalVolume = volumeObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.priority = 1;
            
            // 创建新的Volume Profile
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            globalVolume.profile = profile;
            
            DontDestroyOnLoad(volumeObj);
        }

        // 获取或添加景深效果
        if (globalVolume.profile.TryGet(out depthOfField))
        {
            depthOfField.active = false;
        }
        else
        {
            depthOfField = globalVolume.profile.Add<DepthOfField>();
            depthOfField.active = false;
        }

        // 设置景深模式为高斯模糊
        depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
    }

    /// <summary>
    /// 带模糊效果的场景切换
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(TransitionCoroutine(sceneName));
    }

    /// <summary>
    /// 带模糊效果的场景切换（使用场景索引）
    /// </summary>
    public void TransitionToScene(int sceneIndex)
    {
        StartCoroutine(TransitionCoroutine(sceneIndex));
    }

    private IEnumerator TransitionCoroutine(string sceneName)
    {
        // 启用模糊效果
        yield return StartCoroutine(FadeBlur(0f, maxBlurIntensity, transitionDuration * 0.5f));
        
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

        // 淡出模糊效果
        yield return StartCoroutine(FadeBlur(maxBlurIntensity, 0f, transitionDuration * 0.5f));
        
        // 禁用模糊效果
        depthOfField.active = false;
    }

    private IEnumerator TransitionCoroutine(int sceneIndex)
    {
        // 启用模糊效果
        yield return StartCoroutine(FadeBlur(0f, maxBlurIntensity, transitionDuration * 0.5f));
        
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

        // 淡出模糊效果
        yield return StartCoroutine(FadeBlur(maxBlurIntensity, 0f, transitionDuration * 0.5f));
        
        // 禁用模糊效果
        depthOfField.active = false;
    }

    /// <summary>
    /// 模糊效果渐变
    /// </summary>
    private IEnumerator FadeBlur(float startIntensity, float endIntensity, float duration)
    {
        depthOfField.active = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 使用平滑曲线
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float currentIntensity = Mathf.Lerp(startIntensity, endIntensity, smoothT);
            
            depthOfField.gaussianStart.Override(0f);
            depthOfField.gaussianEnd.Override(currentIntensity);
            depthOfField.gaussianMaxRadius.Override(1f);

            yield return null;
        }

        // 确保最终值准确
        depthOfField.gaussianEnd.Override(endIntensity);
    }
}
