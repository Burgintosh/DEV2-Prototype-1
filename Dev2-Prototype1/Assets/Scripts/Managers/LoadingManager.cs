using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    public Image fillImage;
    public Canvas loadingCanvas;
    public TextMeshProUGUI continuePromptText;
    //public float promptDelay = 3f;

    private Coroutine loadCoroutine;
    private AsyncOperation currentAsyncOperation;

    private const float ActivationTimeoutSeconds = 10f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingCanvas != null)
                loadingCanvas.sortingOrder = 1000;

            if (continuePromptText != null)
            {
                bool isWeb = Application.platform == RuntimePlatform.WebGLPlayer;
                continuePromptText.text = isWeb
                    ? "Scene is loaded. If you're still here, press P to continue."
                    : "Scene is loaded. If you're still here, press ESC to continue.";
                continuePromptText.gameObject.SetActive(false);
            }
            if (loadingCanvas != null) loadingCanvas.gameObject.SetActive(false);

        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartLoading(string sceneName)
    {
        if (currentAsyncOperation != null && !currentAsyncOperation.isDone)
            currentAsyncOperation.allowSceneActivation = true;
        if (loadCoroutine != null)
        {
            StopCoroutine(loadCoroutine);
            loadCoroutine = null;
        }

        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(true);

        if (fillImage != null)
            fillImage.fillAmount = 0f;
        
        if (continuePromptText != null)
            continuePromptText.gameObject.SetActive(false);

        loadCoroutine = StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("LoadingManager: StartLoading called with empty sceneName.");
            yield break;
        }

        Debug.Log($"LoadingManager: begin LoadSceneAsync('{sceneName}')");
        currentAsyncOperation = SceneManager.LoadSceneAsync(sceneName);
        currentAsyncOperation.allowSceneActivation = false;

        while (currentAsyncOperation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(currentAsyncOperation.progress / 0.9f);

            if (fillImage != null && fillImage.gameObject.activeInHierarchy)
                fillImage.fillAmount = progress;

            yield return null;
        }

        if (fillImage != null && fillImage.gameObject.activeInHierarchy)
            fillImage.fillAmount = 1f;

        Debug.Log("LoadingManager: reached 0.9f.");

        //yield return new WaitForSeconds(promptDelay);
        yield return new WaitForSeconds(0.25f);

        continuePromptText.gameObject.SetActive(true);

        currentAsyncOperation.allowSceneActivation = true;

        Debug.Log("After allowing activation.");

        //while (currentAsyncOperation != null && !currentAsyncOperation.isDone)
        //    yield return null;

        if (continuePromptText != null)
            continuePromptText.gameObject.SetActive(false);

        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(false);
        Debug.Log("After deactivating loadingCanvas.");

        currentAsyncOperation = null;
        loadCoroutine = null;
        Debug.Log("End of coroutine.");
    }
}