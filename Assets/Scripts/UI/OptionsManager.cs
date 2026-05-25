using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    public Slider musicSlider;
    public Slider soundSlider;
    public AudioSource backgroundMusic;
    public GameObject optionsPanel;
    public GameObject pauseButton;
    public GameObject playButton;

    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("Music", 1f);
        soundSlider.value = PlayerPrefs.GetFloat("Sound", 1f);

        musicSlider.onValueChanged.AddListener(ChangeMusic);
        soundSlider.onValueChanged.AddListener(ChangeSound);

        ChangeMusic(musicSlider.value);
        ChangeSound(soundSlider.value);

        playButton.SetActive(false);
    }

    public void ChangeMusic(float value)
    {
        backgroundMusic.volume = value;
        PlayerPrefs.SetFloat("Music", value);
    }

    public void ChangeSound(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Sound", value);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        optionsPanel.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        Time.timeScale = 0f;
        pauseButton.SetActive(false);
        playButton.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseButton.SetActive(true);
        playButton.SetActive(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
            {
                optionsPanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
            }
            else
            {
                optionsPanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
            }
        }
    }
}