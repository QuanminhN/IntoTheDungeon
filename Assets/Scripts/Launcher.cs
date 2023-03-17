using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Launcher : MonoBehaviourPunCallbacks
{
    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Sync all clients to the host/master scene
        Connect();
    }

    public override void OnConnectedToMaster()
    {
        Join();

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
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1) //if leader of the room
        {
            PhotonNetwork.LoadLevel(1); //Load up the new scene (Check number in the Build Scene Setting)
        }
    }
}
