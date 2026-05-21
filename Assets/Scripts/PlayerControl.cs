using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PlayerControl : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -20f;
    public float jumpHeight = 1.2f;
    public float passDistance = 3f;
    public float pushForce = 10f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 100f;

    [HideInInspector] public string networkId;

    [Networked] public NetworkBool HasBomb { get; set; }

    public float passRange = 3f;       
    public LayerMask playerLayer;

    private BombController currentBomb;

    private CharacterController _cc;
    private Vector3 _velocity;
    private float _xRotation;

    private Color playerColor;
    void Start()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        playerColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);
        GetComponent<MeshRenderer>().material.color = playerColor;
    }

    void Update()
    {
        MouseLook();
        Movement();
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Movement()
    {
        bool grounded = _cc.isGrounded;
        if (grounded && _velocity.y < 0f) _velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        _cc.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;

        // Presionar F o botón para pasar la bomba
        if (GetInput(out NetworkInputData input) && input.PassBomb)
        {
            TryPassBomb();
        }
    }

    void TryPassBomb()
    {
        if (!HasBomb) return;

        // Buscar jugadores cercanos
        Collider[] nearby = Physics.OverlapSphere(transform.position, passRange, playerLayer);

        PlayerControl closest = null;
        float minDist = float.MaxValue;

        foreach (var col in nearby)
        {
            var other = col.GetComponent<PlayerControl>();
            if (other == null || other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = other;
            }
        }

        if (closest != null)
        {
            RPC_RequestPassBomb(closest.Object.InputAuthority);
        }
        else
        {
            Debug.Log("No hay jugadores cerca para pasar la bomba!");
        }
    }

    // RPC: el cliente le pide al host que transfiera la bomba
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestPassBomb(PlayerRef targetPlayer)
    {
        if (!HasBomb) return;

        // Encontrar la bomba en la escena
        var bomb = FindObjectOfType<BombController>();
        if (bomb == null) return;

        // Quitar bomba de este jugador
        HasBomb = false;

        // Dar bomba al otro jugador
        if (Runner.TryGetPlayerObject(targetPlayer, out NetworkObject targetObj))
        {
            var targetHandler = targetObj.GetComponent<PlayerControl>();
            if (targetHandler != null)
            {
                targetHandler.HasBomb = true;
                bomb.PassBomb(targetPlayer);
                Debug.Log($"Bomba pasada a {targetPlayer}!");
            }
        }
    }

    // Llamado por BombController cuando explota
    public void OnBombExploded()
    {
        HasBomb = false;
        Debug.Log("Perdiste! La bomba explotó en tus manos.");
        // Aquí puedes: mostrar pantalla de derrota, restar vida, etc.
    }

    // Para el host: dar la bomba inicial a este jugador
    public void GiveBomb(BombController bomb)
    {
        if (!Object.HasStateAuthority) return;
        HasBomb = true;
        currentBomb = bomb;
        bomb.BombHolder = Object.InputAuthority;
        bomb.transform.SetParent(transform);
        bomb.transform.localPosition = Vector3.up * 1.5f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, passRange);
    }
}