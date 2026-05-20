using UnityEngine;

public class Void : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        PlayerControl lp = other.GetComponentInParent<PlayerControl>();
        if (lp != null)
        {
            GameManager.Instance.DeletePlayer(lp.networkId);
            return;
        }

        RemotePlayer rp = other.GetComponentInParent<RemotePlayer>();
        if (rp != null)
            GameManager.Instance.DeletePlayer(rp.networkId);
    }
}