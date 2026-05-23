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
            // Rotación horizontal sincronizada con la red
            Yaw += data.MouseX * 2f;
            transform.rotation = Quaternion.Euler(0f, Yaw, 0f);

            Vector3 direction = data.Direction.normalized;
            Vector3 move = transform.right * direction.x + transform.forward * direction.z;
            move *= moveSpeed;

            float verticalVelocity = Velocity.y + gravity * Runner.DeltaTime;

            if (data.Buttons.IsSet(NetworkInputData.JUMP_BUTTON) && _cc.isGrounded)
                verticalVelocity = jumpImpulse;

            Velocity = new Vector3(move.x, verticalVelocity, move.z);
            _cc.Move(Velocity * Runner.DeltaTime);
        }
    }
}