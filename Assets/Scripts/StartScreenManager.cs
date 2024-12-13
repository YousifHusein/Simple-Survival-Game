using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public GameObject mainCanvas;
    public GameObject infoCanvas;

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void Information()
    {
        mainCanvas.SetActive(false);
        infoCanvas.SetActive(true);
    }

    public void Menu()
    {
        mainCanvas.SetActive(true);
        infoCanvas.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
