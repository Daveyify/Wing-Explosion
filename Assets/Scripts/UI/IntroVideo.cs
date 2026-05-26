using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public CanvasGroup fadePanel;
    public CanvasGroup videoPanel;
    public float fadeDuration = 2f;
    public string nextScene = "";

    private const string INTRO_PLAYED_KEY = "IntroVideoPlayed";

    void Start()
    {
        // Si el video ya se reprodujo antes, saltar directo
        if (PlayerPrefs.GetInt(INTRO_PLAYED_KEY, 0) == 1)
        {
            SkipToNextScene();
            return;
        }

        if (fadePanel != null) fadePanel.alpha = 0;
        if (videoPanel != null) videoPanel.alpha = 1;
        videoPlayer.Play();
        StartCoroutine(HandleIntro());
    }

    IEnumerator HandleIntro()
    {
        yield return new WaitUntil(() => videoPlayer.isPlaying);
        yield return new WaitUntil(() => !videoPlayer.isPlaying);

        // Marcar que el video ya se reprodujo
        PlayerPrefs.SetInt(INTRO_PLAYED_KEY, 1);
        PlayerPrefs.Save();

        // Fade out
        float t = -2;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (fadePanel != null) fadePanel.alpha = t / fadeDuration;
            if (videoPanel != null) videoPanel.alpha = 1 - (t / fadeDuration);
            yield return null;
        }

        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
        else
        {
            if (videoPanel != null) videoPanel.gameObject.SetActive(false);
            if (fadePanel != null) fadePanel.gameObject.SetActive(false);
        }
    }

    void SkipToNextScene()
    {
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
        else
        {
            if (videoPanel != null) videoPanel.gameObject.SetActive(false);
            if (fadePanel != null) fadePanel.gameObject.SetActive(false);
        }
    }
}