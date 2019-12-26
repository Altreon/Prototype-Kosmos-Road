using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject PauseMenuUI;
    public GameObject BoxToQuit;
    public Button[] buttons;
    public Button boxButton;		public TimeManager timeManager;

    public EventSystem myEventSystem;

    public string MenuScene;

    private void Start()
    {
        PauseMenuUI.SetActive(false);
        BoxToQuit.SetActive(false);
        myEventSystem.SetSelectedGameObject(buttons[0].gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause")){
            if (GameIsPaused)
            {
                Resume();
            }
            else {
                enableButtons(true);
                myEventSystem.SetSelectedGameObject(buttons[0].gameObject);
                Pause();
            }

        }

    }

    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        BoxToQuit.SetActive(false);
        timeManager.Resume();
        GameIsPaused = false;
    }

    void Pause()
    {
        PauseMenuUI.SetActive(true);
        timeManager.Pause();
        GameIsPaused = true;
    }

    public void LoadToMenu()
    {
        Debug.Log("Go to menu ...");
        SceneManager.LoadScene(MenuScene);
    }

    public void Quit()
    {
        Debug.Log("Quitting the game ...");
        if(BoxToQuit.activeSelf == true)
        {
            Debug.Log("The game is officially off");
            Application.Quit();
        } else
        {
            enableButtons(false);
            myEventSystem.SetSelectedGameObject(boxButton.gameObject);
            BoxToQuit.SetActive(true);
        }
    }

    public void DontQuit()
    {
        enableButtons(true);
        BoxToQuit.SetActive(false);
        myEventSystem.SetSelectedGameObject(buttons[0].gameObject);
    }

    void enableButtons(bool b)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(b);
        }
    }
}
