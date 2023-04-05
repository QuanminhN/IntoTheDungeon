using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

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

    //Tabs
    public GameObject mainTab;
    public GameObject roomsTab;
    public GameObject createTab;

    public GameObject roomBottonPrefab;

    //Create room
    public TMP_InputField roomNameField;
    public Slider maxPlayersSlider;
    public TMP_Text maxPlayerValue;

    private List<RoomInfo> roomList;
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

        PhotonNetwork.JoinLobby();
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
        RoomOptions options = new RoomOptions(); //Used to set room settings
        options.MaxPlayers = (byte)maxPlayersSlider.value;

        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
        properties.Add("Map", 0);
        options.CustomRoomProperties = properties;

        PhotonNetwork.CreateRoom(roomNameField.text, options);
    }

    public void ChangeMap()
    {

    }

    public void ChangeMaxPlayerSlider(float t_val)
    {
        maxPlayerValue.text = Mathf.RoundToInt(t_val).ToString();
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

        VerifyUsername();

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1) //if leader of the room
        {
            Data.SaveProfile(myProfile);
            PhotonNetwork.LoadLevel(1); //Load up the new scene (Check number in the Build Scene Setting)
        }
    }

    public void TabCloseAll()
    {
        mainTab.SetActive(false);
        roomsTab.SetActive(false);
        createTab.SetActive(false);
    }

    public void OpenMainTab()
    {
        TabCloseAll();
        mainTab.SetActive(true);
    }
    public void OpenRoomTab()
    {
        TabCloseAll();
        roomsTab.SetActive(true);
    }

    public void OpenCreateTab()
    {
        TabCloseAll();
        createTab.SetActive(true);
    }

    private void ClearRoomList()
    {
        Transform contents = roomsTab.transform.Find("Scroll View/Viewport/Content");
        foreach(Transform content in contents)
        {
            Debug.Log("DELETE");
            Destroy(content.gameObject);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> t_roomList)
    {
        roomList = t_roomList;
        ClearRoomList();

        Debug.Log("LOAD ROOMS @ " + Time.time);
        Transform content = roomsTab.transform.Find("Scroll View/Viewport/Content");

        foreach(RoomInfo room in roomList)
        {
            GameObject newRoomButton = Instantiate(roomBottonPrefab, content);
            newRoomButton.transform.Find("Name").GetComponent<Text>().text = room.Name;
            newRoomButton.transform.Find("Capacity").GetComponent<Text>().text = room.PlayerCount + " / " + room.MaxPlayers;

            newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
        }

        base.OnRoomListUpdate(roomList);
    }

    public void JoinRoom(Transform t_button)
    {
        VerifyUsername();

        //Debug.Log("JOIN ROOM @ " + Time.time);
        string t_roomName = t_button.transform.Find("Name").GetComponent<TextMeshPro>().text;
        PhotonNetwork.JoinRoom(t_roomName);
    }

    private void VerifyUsername()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            if (string.IsNullOrEmpty(usernameField.text))
            {
                myProfile.username = "RANDOM_USER" + Random.Range(1000, 9999).ToString();
            }
            else
            {
                myProfile.username = usernameField.text;
            }
        }
    }
}
