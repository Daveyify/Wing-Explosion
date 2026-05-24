using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private CharacterController _cc;
    private Vector3 _forward = Vector3.forward;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpImpulse = 8f;
    [SerializeField] private float gravity = -20f;

    [Networked] private Vector3 Velocity { get; set; }

    public Vector3 CurrentVelocity => Velocity;

    [Networked] private float Yaw { get; set; }

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        GetComponent<MeshRenderer>().material.color =
            Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Vector3 direction = data.Direction.normalized;
            Vector3 move = transform.right * direction.x + transform.forward * direction.z;
            move *= moveSpeed;

            float verticalVelocity = Velocity.y + gravity * Runner.DeltaTime;

            if (_cc.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (data.Buttons.IsSet(NetworkInputData.JUMP_BUTTON) && _cc.isGrounded)
                verticalVelocity = jumpImpulse;

            Velocity = new Vector3(move.x, verticalVelocity, move.z);
            _cc.Move(Velocity * Runner.DeltaTime);

            Yaw += data.MouseX * 2f;
            transform.rotation = Quaternion.Euler(0f, Yaw, 0f);

            if (direction.sqrMagnitude > 0)
                _forward = direction;
        }
    }
}