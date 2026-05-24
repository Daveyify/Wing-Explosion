using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BombManager : NetworkBehaviour
{
    [SerializeField] private float bombTimer = 10f;

    [Networked] public PlayerRef BombHolder { get; private set; }
    [Networked] public float TimeLeft { get; private set; }
    [Networked] public bool GameActive { get; private set; }

    [Networked, Capacity(8), OnChangedRender(nameof(OnAlivePlayersChanged))]
    public NetworkLinkedList<PlayerRef> AlivePlayers { get; }

    public static BombManager Instance { get; private set; }

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
        Debug.Log($"¡Boom! Jugador {BombHolder} eliminado");
        AlivePlayers.Remove(BombHolder);

        if (AlivePlayers.Count <= 1)
        {
            GameActive = false;
            Debug.Log($"¡Ganó el jugador {AlivePlayers[0]}!");
            return;
        }

        TimeLeft = bombTimer;
        var remaining = AlivePlayers.ToList();
        BombHolder = remaining[Random.Range(0, remaining.Count)];
    }
}