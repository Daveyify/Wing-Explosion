using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const int JUMP_BUTTON = 0;
    public const int PASS_BUTTON = 1;

    public NetworkButtons Buttons;
    public Vector3 Direction;

    public NetworkBool Jump;
    public NetworkBool PassBomb;

    public float MouseX;
}
