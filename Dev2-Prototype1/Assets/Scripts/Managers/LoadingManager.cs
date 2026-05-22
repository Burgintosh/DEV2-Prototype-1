using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    public Image fillImage;
    public Canvas loadingCanvas;

    private Coroutine loadCoroutine;
    private AsyncOperation currentAsyncOperation;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingCanvas != null)
                loadingCanvas.sortingOrder = 1000;

        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartLoading(string sceneName)
    {
        if (currentAsyncOperation != null && !currentAsyncOperation.isDone)
        {
            currentAsyncOperation.allowSceneActivation = true;
        }
        if (loadCoroutine != null)
        {
            StopCoroutine(loadCoroutine);
            loadCoroutine = null;
        }

        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(true);

        if (fillImage != null)
            fillImage.fillAmount = 0f;

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

        yield return new WaitForSeconds(0.25f);
        Debug.Log("Yielded Wait for Seconds.");

        if (currentAsyncOperation != null)
            currentAsyncOperation.allowSceneActivation = true;

        Debug.Log("After allowing activation.");

        //while (currentAsyncOperation != null && !currentAsyncOperation.isDone)
        //    yield return null;

        if (loadingCanvas != null)
            loadingCanvas.gameObject.SetActive(false);
        Debug.Log("After deactivating loadingCanvas.");

        currentAsyncOperation = null;
        loadCoroutine = null;
        Debug.Log("End of coroutine.");
    }
}