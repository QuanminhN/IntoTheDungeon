using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class PlayerData
{
    public string username;
    public int level;
    public int xp;

    public PlayerData(string u, int l, int x)
    {
        username = u;
        level = l;
        xp = x;
    }
    public PlayerData()
    {
        username = "";
        level = 0;
        xp = 0;
    }
}

public class Launcher : MonoBehaviourPunCallbacks
{
    public static PlayerData myProfile = new PlayerData();
    public TMP_InputField usernameField;
    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Sync all clients to the host/master scene

        myProfile = Data.LoadProfile();
        usernameField.text = myProfile.username;
        Connect();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("CONNECTED TO MASTER");
        base.OnConnectedToMaster(); //This calls the base function
    }

    public override void OnJoinedRoom() //Successfully joined a room
    {
        StartGame();


        base.OnJoinedRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message) //Failed to join a random room a.k.a no rooms
    {
        CreateNewRoom();

        base.OnJoinRandomFailed(returnCode, message);
    }

    public void CreateNewRoom()
    {
        PhotonNetwork.CreateRoom("");
    }

    public void Connect()
    {
        PhotonNetwork.GameVersion = "0.0.0"; //Restrict players to join other players to the same version
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Join()
    {
        PhotonNetwork.JoinRandomRoom(); //Join a room 
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            myProfile.username = "RANDOM_USER" + Random.Range(1000, 9999).ToString();
        }
        else
        {
            myProfile.username = usernameField.text;
        }
        
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1) //if leader of the room
        {
            Data.SaveProfile(myProfile);
            PhotonNetwork.LoadLevel(1); //Load up the new scene (Check number in the Build Scene Setting)
        }
    }
}
