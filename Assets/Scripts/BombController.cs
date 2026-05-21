using Fusion;
using UnityEngine;

public class BombController : NetworkBehaviour
{
    [Networked] public TickTimer ExplosionTimer { get; set; }
    [Networked] public PlayerRef BombHolder { get; set; }

    public float bombDuration = 10f; 

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            ExplosionTimer = TickTimer.CreateFromSeconds(Runner, bombDuration);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (ExplosionTimer.Expired(Runner))
        {
            Explode();
        }
    }

    void Explode()
    {
        Debug.Log($"Jugador eliminado: {BombHolder}");

        if (Runner.TryGetPlayerObject(BombHolder, out NetworkObject playerObj))
        {
            playerObj.GetComponent<PlayerControl>()?.OnBombExploded();
        }

        Runner.Despawn(Object);
    }

    public void PassBomb(PlayerRef newHolder)
    {
        if (!Object.HasStateAuthority) return;

        BombHolder = newHolder;

        if (Runner.TryGetPlayerObject(newHolder, out NetworkObject playerObj))
        {
            transform.SetParent(playerObj.transform);
            transform.localPosition = Vector3.up * 1.5f;
        }
    }

    public float TimeLeft => ExplosionTimer.RemainingTime(Runner) ?? 0f;
}