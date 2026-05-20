using UnityEngine;

public class PlayerControl : MonoBehaviour
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

        if (Input.GetMouseButtonDown(0))
            TryPunch();
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

    void TryPunch()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * passDistance, Color.red, 1f);

        if (!Physics.Raycast(ray, out RaycastHit hit, passDistance)) return;

        Vector3 pushDir = (hit.collider.transform.position - transform.position).normalized;
        pushDir.y = 0.3f;
        pushDir.Normalize();

        PlayerControl localTarget = hit.collider.GetComponentInParent<PlayerControl>();
        if (localTarget != null && localTarget != this)
        {
            localTarget.RecievePush(pushDir);
            NetworkManager.Instance.BroadcastPush(localTarget.networkId, pushDir);
            return;
        }

        RemotePlayer remoteTarget = hit.collider.GetComponentInParent<RemotePlayer>();
        if (remoteTarget != null)
            NetworkManager.Instance.BroadcastPush(remoteTarget.networkId, pushDir);
    }

    public void RecievePush(Vector3 direction)
    {
        _velocity += direction* pushForce;
    }

    public string GetColorString() => $"{playerColor.r:F2}|{playerColor.g:F2}|{playerColor.b:F2}";

    public float GetCameraXRotation() => _xRotation;
}