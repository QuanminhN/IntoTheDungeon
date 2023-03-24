using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Using UnityEngine.SceneManagement //needed for single player

public class MainMenu : MonoBehaviour
{

    [SerializeField]private Launcher launcher;

    private void Start()
    {
        Pause.paused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void JoinMatch()
    {
        //SceneManager.LoadScene(); // For single player
        launcher.Join();
    }
    public void CreateMatch()
    {
        launcher.CreateNewRoom();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
