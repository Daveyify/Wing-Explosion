using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void StartHost()
    {
        PlayerPrefs.SetString("GameMode", "Host");
        SceneManager.LoadScene("MainGame");
    }

    public void StartClient()
    {
        PlayerPrefs.SetString("GameMode", "Client");
        SceneManager.LoadScene("MainGame");
    }
}