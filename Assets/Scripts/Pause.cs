using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Pause : MonoBehaviour
{
    public static bool paused = false;
    private bool disconnecting = false;

    public void TogglePause()
    {
        if (disconnecting) return;

        paused = !paused; // Toggle paused

        transform.GetChild(0).gameObject.SetActive(paused);
        Cursor.lockState = (paused) ? CursorLockMode.None : CursorLockMode.Confined;
        Cursor.visible = paused;
    }

    public void quitGame()
    {
        Debug.Log("QUIT GAME");
        disconnecting = true;
        PhotonNetwork.LeaveRoom();
        StartCoroutine(goToMainScene()); //Fixes error when player leaves immedaiately
    }

    IEnumerator goToMainScene()
    {
        yield return new WaitForSeconds(.1f);
        SceneManager.LoadScene(0);
    }
}
