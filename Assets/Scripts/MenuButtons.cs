using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public GameObject MenuPanel;
    
    void Start()
    {
        MenuPanel.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("TitleScreen");
    }

    public void GoToHighscore()
    {
        SceneManager.LoadScene("HighscoreScreen");
    }
}
