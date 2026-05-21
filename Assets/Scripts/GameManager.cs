using System.Collections.Generic;
using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class GameManager : NetworkBehaviour
{
    public NetworkPrefabRef bombPrefab;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        StartCoroutine(SpawnBombDelayed());
    }

    System.Collections.IEnumerator SpawnBombDelayed()
    {
        yield return new WaitForSeconds(1f);

        var players = new List<PlayerRef>(Runner.ActivePlayers);
        if (players.Count == 0) yield break;

        PlayerRef startPlayer = players[Random.Range(0, players.Count)];

        NetworkObject bombObj = Runner.Spawn(bombPrefab, Vector3.zero, Quaternion.identity);
        BombController bomb = bombObj.GetComponent<BombController>();

        if (Runner.TryGetPlayerObject(startPlayer, out NetworkObject playerObj))
        {
            playerObj.GetComponent<PlayerControl>()?.GiveBomb(bomb);
        }

        Debug.Log($"🎮 Juego iniciado! {startPlayer} tiene la bomba.");
    }
} 