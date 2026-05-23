using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkPrefabRef bombPrefab;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
    }
}