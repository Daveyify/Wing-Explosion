using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Admin Panel")]
    public GameObject adminPanel;
    public GameObject pauseMenu;
    public Button adminPanelButton;
    public Transform playerListContainer;
    public GameObject playerEntryPrefab;

    [Header("HUD")]
    public TextMeshProUGUI pingText;     

    private bool isPaused = false;
    private bool isAdminOpen = false;
    private float pingTimer = 0f;
    private float pingInterval = 2f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (adminPanel != null) adminPanel.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (adminPanelButton != null)
            adminPanelButton.interactable = NetworkManager.Instance.isHost;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

            if (NetworkManager.Instance.isHost)
                NetworkManager.Instance?.BroadcastPause(isPaused);
        }

        pingTimer += Time.deltaTime;
        if (pingTimer >= pingInterval)
        {
            pingTimer = 0f;
            NetworkManager.Instance?.SendPing();
        }
    }

    public void ToggleAdminPanel()
    {
        isAdminOpen = !isAdminOpen;
        adminPanel.SetActive(isAdminOpen);
        Cursor.lockState = isAdminOpen ? CursorLockMode.None : CursorLockMode.Locked;

        if (isAdminOpen)
        {
            RefreshPlayerList();
            NetworkManager.Instance?.BroadcastPause(true);
        }
        else
        {
            NetworkManager.Instance?.BroadcastPause(false);
        }
    }

    public void RefreshPlayerList()
    {
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        bool isHost = NetworkManager.Instance.isHost;

        foreach (var kvp in NetworkManager.Instance.GetPings())
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI pingTxt = entry.transform.Find("PingText").GetComponent<TextMeshProUGUI>();
            Button kickBtn = entry.transform.Find("KickButton").GetComponent<Button>();

            nameText.text = kvp.Key == NetworkManager.Instance.localId ? kvp.Key + " (tú)" : kvp.Key;
            pingTxt.text = kvp.Value + " ms";

            if (isHost && kvp.Key != NetworkManager.Instance.localId)
            {
                kickBtn.gameObject.SetActive(true);
                string idToKick = kvp.Key;
                kickBtn.onClick.AddListener(() => KickPlayer(idToKick));
            }
            else
            {
                kickBtn.gameObject.SetActive(false);
            }
        }
    }

    private void KickPlayer(string id)
    {
        Debug.Log("[UI] Kicking player: " + id);
        NetworkManager.Instance.BroadcastKick(id);
        GameManager.Instance.HandleDelete(id);
    }

    public void HandlePause(bool pause)
    {
        if (!NetworkManager.Instance.isHost)
        {
            isPaused = pause;
            adminPanel.SetActive(pause);
            Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void UpdateOwnPing(int ms)
    {
        if (pingText != null)
            pingText.text = "Ping: " + ms + " ms";
    }

    public void ChangeScene(string sceneName) => SceneManager.LoadScene(sceneName);
    public void Exit() => Application.Quit();
}