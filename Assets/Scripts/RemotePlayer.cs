using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public string networkId;

    [Header("Bomb")]
    public GameObject bombModel;

    private Vector3 targetPos;
    private float targetRx, targetRy;
    private bool hasTarget = false;

    public float interpolationSpeed = 15f;

    void Start()
    {
        targetPos = transform.position;
        if (bombModel != null) bombModel.SetActive(false);
    }

    void Update()
    {
        if (!hasTarget) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            interpolationSpeed * Time.deltaTime
        );

        Quaternion targetRot = Quaternion.Euler(0f, targetRy, 0f);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            interpolationSpeed * 10f * Time.deltaTime
        );
    }

    public void SetTarget(Vector3 pos, float rx, float ry)
    {
        targetPos = pos;
        targetRx = rx;
        targetRy = ry;
        hasTarget = true;
    }

    public void ShowBomb(bool show)
    {
        if (bombModel != null) bombModel.SetActive(show);
    }
    public void SetColor(float r, float g, float b)
    {
        GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }
}