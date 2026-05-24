using Fusion;
using UnityEngine;
using TMPro;

public class LobbyUI : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI waitingText;

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
            waitingText.text = "Presiona [ENTER] para iniciar";

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (count < 2)
                    waitingText.text = "¡Necesitas al menos 2 jugadores!";
                else
                {
                    BombManager.Instance.StartGame();
                    lobbyPanel.SetActive(false);
                }
            }
        }
        else
        {
            waitingText.text = "Esperando que el host inicie...";
        }
    }
}