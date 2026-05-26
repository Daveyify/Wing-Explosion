using Fusion;
using UnityEngine;
using TMPro;

public class LobbyUI : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private GameObject hostImage;
    [SerializeField] private GameObject clientImage;

    private void Update()
    {
        if (BombManager.Instance == null) return;

        int count = BombManager.Instance.AlivePlayers.Count;
        playersText.text = $"Jugadores listos: {count}/8";

        if (BombManager.Instance.GameActive)
        {
            lobbyPanel.SetActive(false);
            return;
        }

        if (Runner.IsServer)
        {
            hostImage.SetActive(true);
            clientImage.SetActive(false);

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (count >= 2)
                {
                    BombManager.Instance.StartGame();
                    lobbyPanel.SetActive(false);
                }
            }
        }
        else
        {
            hostImage.SetActive(false);
            clientImage.SetActive(true);
        }
    }
}