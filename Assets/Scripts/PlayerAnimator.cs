using Fusion;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private Animator _animator;
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public override void Render()
    {
        if (_animator == null)
        {
            Debug.Log("Animator es NULL");
            return;
        }

        var player = GetComponent<Player>();
        if (player == null)
        {
            Debug.Log("Player es NULL");
            return;
        }

        Vector3 vel = player.CurrentVelocity;
        bool isMoving = new Vector2(vel.x, vel.z).magnitude > 0.1f;


        _animator.SetBool(IsMoving, isMoving);
    }
}