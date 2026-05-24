using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class BombInteraction : NetworkBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private string bombModelPath = "Camera/HandSocket/BombModel";
    [SerializeField] private string playerMeshPath = "PlayerMesh"; 

    private Camera _playerCamera;
    private GameObject _bombModel;
    private GameObject _playerMesh;
    private LayerMask _playerLayer;

    public override void Spawned()
    {
        _playerCamera = GetComponentInChildren<Camera>();
        _bombModel = transform.Find(bombModelPath)?.gameObject;
        _playerMesh = transform.Find(playerMeshPath)?.gameObject;
        _playerLayer = LayerMask.GetMask("Player");

        if (HasStateAuthority)
            BombManager.Instance.RegisterPlayer(Object.InputAuthority);

        if (_playerCamera != null)
        {
            bool isMine = HasInputAuthority;
            _playerCamera.enabled = isMine;
            var listener = _playerCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = isMine;
        }

        if (_bombModel != null) _bombModel.SetActive(false);

        Debug.Log($"Spawned player - IsMine: {HasInputAuthority} - Camera: {_playerCamera != null} - Bomb: {_bombModel != null} - Mesh: {_playerMesh != null}");
    }


    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        if (BombManager.Instance == null || !BombManager.Instance.GameActive) return;
        if (BombManager.Instance.BombHolder != Runner.LocalPlayer) return;

        if (GetInput(out NetworkInputData data))
        {
            if (data.Buttons.IsSet(NetworkInputData.PASS_BUTTON))
            {
                Debug.Log("¡Click recibido! Intentando pasar bomba...");
                TryPassBomb();
            }
        }
    }

    private void TryPassBomb()
    {
        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, _playerLayer))
        {
            Debug.Log($"Hit: {hit.collider.name} - Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            var targetPlayer = hit.collider.GetComponent<BombInteraction>();
            if (targetPlayer != null && targetPlayer != this)
                RPC_PassBomb(Runner.LocalPlayer, targetPlayer.Object.InputAuthority);
            else
                Debug.Log("No tiene BombInteraction o es el mismo jugador");
        }
        else
        {
            Debug.Log("Raycast no golpeó nada en layer Player");
        }
    }

    public void UpdateVisuals()
    {
        if (BombManager.Instance == null) return;

        bool isAlive = false;
        foreach (var p in BombManager.Instance.AlivePlayers)
        {
            if (p == Object.InputAuthority)
            {
                isAlive = true;
                break;
            }
        }

        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            if (!isAlive)
                meshRenderer.enabled = false;       
            else if (HasInputAuthority)
                meshRenderer.enabled = false;       
            else
                meshRenderer.enabled = true;        
        }

        if (_bombModel != null)
        {
            bool hasBomb = BombManager.Instance.BombHolder == Object.InputAuthority && isAlive;
            _bombModel.SetActive(hasBomb);
        }
    }

    public override void Render()
    {
        if (BombManager.Instance == null) return;
        UpdateVisuals();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PassBomb(PlayerRef from, PlayerRef to)
    {
        BombManager.Instance.TryPassBomb(from, to);
    }
}