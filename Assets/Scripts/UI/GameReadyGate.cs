using UnityEngine;
using System.Collections;

public class GameReadyGate : MonoBehaviour
{
    [SerializeField] private GameObject _loadingCanvas;
    [SerializeField] private GameObject _instructionsPanel;

    void Start()
    {
        _loadingCanvas.SetActive(true);
        _instructionsPanel.SetActive(false);

        StartCoroutine(WaitForFusion());
    }

    IEnumerator WaitForFusion()
    {
        yield return new WaitUntil(() => BasicSpawner.IsReady);

        _loadingCanvas.SetActive(false);
        _instructionsPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}