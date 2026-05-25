using Fusion;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFollow : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    private float _rotationX = 0f;
    private Transform _playerBody;

    public override void Spawned()
    {
        _playerBody = transform.parent;

        if (HasInputAuthority)
        {
            GetComponent<Camera>().enabled = true;
            GetComponent<AudioListener>().enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))


        if (!HasInputAuthority) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -maxLookAngle, maxLookAngle);
        transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
    }

}