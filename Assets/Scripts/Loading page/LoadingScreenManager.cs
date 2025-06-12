using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenManager : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text loadingText;

    private void Start()
    {
        LoadSceneWithAssets("GameScene");
    }
    public void LoadSceneWithAssets(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        // Start loading the scene asynchronously
        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(sceneName);
        sceneLoading.allowSceneActivation = false;

        float progress = 0f;

        while (!sceneLoading.isDone)
        {
            // Scene loading progress goes from 0 to 0.9
            progress = Mathf.Clamp01(sceneLoading.progress / 0.9f);
            progressBar.fillAmount = progress;

            if (loadingText != null)
                loadingText.text = $"Loading... {(int)(progress * 100)}%";

            // When progress reaches 0.9, we manually activate the scene
            if (sceneLoading.progress >= 0.9f)
            {
                progressBar.fillAmount = 1f;
                if (loadingText != null)
                    loadingText.text = "Loading... 100%";

                yield return new WaitForSeconds(0.5f); // Optional wait
                sceneLoading.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}