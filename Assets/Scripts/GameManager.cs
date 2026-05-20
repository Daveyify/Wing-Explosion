using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private readonly Dictionary<string, PlayerControl> localPlayers = new();
    private readonly Dictionary<string, RemotePlayer> remotePlayers = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayer(string id, PlayerControl local, RemotePlayer remote = null)
    {
        if (local != null) localPlayers[id] = local;
        if (remote != null) remotePlayers[id] = remote;
    }

    public void UnregisterPlayer(string id)
    {
        localPlayers.Remove(id);
        remotePlayers.Remove(id);
    }

    public void DeletePlayer(string id)
    {
        NetworkManager.Instance?.BroadcastEliminar(id);
        HandleDelete(id);
    }

    public void HandleDelete(string id)
    {
        Debug.Log($"[Game] Player {id} eliminated");

        if (localPlayers.TryGetValue(id, out PlayerControl lp))
            lp.gameObject.SetActive(false);
        else if (remotePlayers.TryGetValue(id, out RemotePlayer rp))
            rp.gameObject.SetActive(false);

        UnregisterPlayer(id);
        CheckVictory();
    }

    private void CheckVictory()
    {
        int total = localPlayers.Count + remotePlayers.Count;
        if (total <= 1)
            SceneManager.LoadScene("WinScene");
    }

    public PlayerControl GetLocalPlayer(string id)
    {
        localPlayers.TryGetValue(id, out PlayerControl lp);
        return lp;
    }
} 