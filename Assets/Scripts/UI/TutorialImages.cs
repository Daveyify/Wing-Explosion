using UnityEngine;

public class TutorialPanels : MonoBehaviour
{
    public GameObject[] panels;
    public GameObject mainPanel;

    private int currentPanel = 0;

    void Start()
    {
        ShowPanel(currentPanel);
    }

    void Update()
    {
        // Cambiar de panel con la tecla L
        if (Input.GetKeyDown(KeyCode.L))
        {
            NextPanel();
        }
    }

    public void NextPanel()
    {
        panels[currentPanel].SetActive(false);

        currentPanel++;

        if (currentPanel >= panels.Length)
        {
            mainPanel.SetActive(false);
            return;
        }

        ShowPanel(currentPanel);
    }

    void ShowPanel(int index)
    {
        panels[index].SetActive(true);
    }
}