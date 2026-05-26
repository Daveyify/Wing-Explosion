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
        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextPanel();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            PreviousPanel();
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

    public void PreviousPanel()
    {
        if (currentPanel <= 0) return;

        panels[currentPanel].SetActive(false);
        currentPanel--;
        ShowPanel(currentPanel);
    }

    void ShowPanel(int index)
    {
        panels[index].SetActive(true);
    }
}