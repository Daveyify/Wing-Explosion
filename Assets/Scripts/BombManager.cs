using Fusion;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;

public class BombManager : NetworkBehaviour
{
    [SerializeField] private float bombTimer = 10f;

    [Networked] public PlayerRef BombHolder { get; private set; }
    [Networked] public float TimeLeft { get; private set; }
    [Networked] public bool GameActive { get; private set; }

    [Networked, Capacity(8), OnChangedRender(nameof(OnAlivePlayersChanged))]
    public NetworkLinkedList<PlayerRef> AlivePlayers { get; }

    public static BombManager Instance { get; private set; }
    public TextMeshProUGUI textoTimer;

    [Networked] public NetworkBool IsHost { get; private set; }

    public override void Spawned()
    {
        Instance = this;
        if (HasStateAuthority)
        {
            GameActive = false;
            IsHost = true;
        }
    }

    public override void Render()
    {
        if (textoTimer != null && GameActive)
            textoTimer.text = $"{TimeLeft:F1}s";
    }

    private void OnAlivePlayersChanged()
    {
        foreach (var bi in FindObjectsOfType<BombInteraction>())
            bi.UpdateVisuals();
    }

    public void StartGame()
    {
        if (!HasStateAuthority) return;
        TimeLeft = bombTimer;
        GameActive = true;

        var players = AlivePlayers.ToList();
        if (players.Count > 0)
            BombHolder = players[Random.Range(0, players.Count)];
    }

    public void RegisterPlayer(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        AlivePlayers.Add(player);
    }

    public void UnregisterPlayer(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        AlivePlayers.Remove(player);
    }

    public void TryPassBomb(PlayerRef from, PlayerRef to)
    {
        if (!HasStateAuthority) return;
        if (BombHolder != from) return;
        if (!AlivePlayers.Contains(to)) return;
        BombHolder = to;
        Debug.Log($"Bomba pasada de {from} a {to}");
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !GameActive) return;
        TimeLeft -= Runner.DeltaTime;
        if (TimeLeft <= 0f)
            ExplodeCurrentHolder();
    }

    private void ExplodeCurrentHolder()
    {
        PlayerRef eliminated = BombHolder;
        Debug.Log($"Boom! Jugador {eliminated} eliminado");

        AlivePlayers.Remove(eliminated);

        if (AlivePlayers.Count <= 1)
        {
            GameActive = false;
            BombHolder = PlayerRef.None;

            if (AlivePlayers.Count == 1)
            {
                Debug.Log($"Ganó el jugador {AlivePlayers[0]}!");
                RPC_LoadSceneForPlayer(eliminated, 2);   
                RPC_LoadSceneForPlayer(AlivePlayers[0], 3);  
            }
            else
            {
                RPC_LoadScene(2); 
            }
            return;
        }

        TimeLeft = bombTimer;
        var remaining = new List<PlayerRef>();
        foreach (var p in AlivePlayers)
            remaining.Add(p);
        BombHolder = remaining[Random.Range(0, remaining.Count)];
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_LoadSceneForPlayer(PlayerRef target, int sceneIndex)
    {
        if (Runner.LocalPlayer == target)
            SceneManager.LoadScene(sceneIndex);
    }
}