using Fusion;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Mouse")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;

    // Rotación acumulada — se lee en FixedUpdateNetwork
    private float _yaw;    // horizontal (cuerpo)
    private float _pitch;  // vertical (cámara)

    [Networked] public NetworkBool HasBomb { get; set; }

    public float passRange = 3f;
    public LayerMask playerLayer;
    private BombController currentBomb;

    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
        _yaw = transform.eulerAngles.y;
        GetComponent<MeshRenderer>().material.color =
            Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);

        if (HasInputAuthority)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Solo acumulamos el input del mouse — NO aplicamos rotación aquí
        if (!HasInputAuthority || playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -90f, 90f);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        if (!GetInput(out NetworkInputData input)) return;

        // Aplicar rotación aquí donde Fusion no la pisa
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

        // Movimiento
        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        Vector3 move = transform.right * input.Direction.x + transform.forward * input.Direction.z;
        controller.Move(move * walkSpeed * Runner.DeltaTime);

        if (input.Jump && grounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Runner.DeltaTime;
        controller.Move(velocity * Runner.DeltaTime);

        if (input.PassBomb)
            TryPassBomb();
    }

    void TryPassBomb()
    {
        if (!HasBomb) return;

        Collider[] nearby = Physics.OverlapSphere(transform.position, passRange, playerLayer);
        PlayerControl closest = null;
        float minDist = float.MaxValue;

        foreach (var col in nearby)
        {
            var other = col.GetComponent<PlayerControl>();
            if (other == null || other == this) continue;
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < minDist) { minDist = dist; closest = other; }
        }

        if (closest != null)
            RPC_RequestPassBomb(closest.Object.InputAuthority);
        else
            Debug.Log("No hay jugadores cerca!");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestPassBomb(PlayerRef targetPlayer)
    {
        if (!HasBomb) return;

        var bomb = FindObjectOfType<BombController>();
        if (bomb == null) return;

        HasBomb = false;

        if (Runner.TryGetPlayerObject(targetPlayer, out NetworkObject targetObj))
        {
            var targetHandler = targetObj.GetComponent<PlayerControl>();
            if (targetHandler != null)
            {
                targetHandler.HasBomb = true;
                bomb.PassBomb(targetPlayer);
            }
        }
    }

    public void OnBombExploded()
    {
        HasBomb = false;
        Debug.Log("Perdiste! La bomba explotó en tus manos.");
    }

    public void GiveBomb(BombController bomb)
    {
        if (!Object.HasStateAuthority) return;
        HasBomb = true;
        currentBomb = bomb;
        bomb.BombHolder = Object.InputAuthority;
        bomb.transform.SetParent(transform);
        bomb.transform.localPosition = new Vector3(0.5f, 1.5f, 0.8f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, passRange);
    }
}