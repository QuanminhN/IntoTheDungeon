using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using TMPro;

public class PlayerInfo
{
    //Keep data of player in the match
    public PlayerData profile;
    public int actor;
    public short kills;
    public short deaths;
    public bool awayTeam; //Change this to like CT/T etc;

    public PlayerInfo(PlayerData t_profile, int t_act, short t_kills, short t_deaths, bool t_team)
    {
        profile = t_profile;
        actor = t_act;
        kills = t_kills;
        deaths = t_deaths;
        awayTeam = t_team;
    }
}

public enum GameState
{
    Waiting = 0,
    Starting = 1,
    Playing = 2,
    Ending = 3,
}

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region Fields
    public GameObject player_prefab;
    public string player_prefab_string;
    public Transform[] spawns;

    public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
    public int myId;

    private int numOfFields = 7;

    private Text ui_mykills;
    private Text ui_myDeaths;
    private Text ui_timer;
    [SerializeField] private GameObject Canvas;

    private Transform ui_leaderboard;
    [SerializeField]private Transform ui_endGame;


    public int mainMenu = 0; //Keeps track of main menu scene 
    public int killCount = 3; // How many kills before ending game

    public GameObject mapCam;

    private GameState state = GameState.Waiting;

    private int matchLength = 180;
    private int currentTime;
    private Coroutine timerCoroutine;
    #endregion

    #region Enums
    public enum EventCodes : byte
    {
        NewPlayer,
        UpdatePlayers,
        ChangeStat,
        RefreshTimer,
    }

    #endregion

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {

        mapCam.SetActive(false);

        ValidateConnection();
        InitUI();
        InitMatchTimer();
        SetActiveToFalse();
        NewPlayer_S(Launcher.myProfile);
        Spawn();
    }

    private void Update()
    {
        if (state == GameState.Ending) return; //No longer need to keep updating game scene

        if (Input.GetKey(KeyCode.Tab))
        { 
            LeaderBoard(ui_leaderboard);
        }
        else
        {
            if (ui_leaderboard.gameObject.activeSelf) 
                ui_leaderboard.gameObject.SetActive(false);
        }
    }

    private void InitUI()
    {
        ui_mykills = GameObject.Find("HUD/Kills/Text").GetComponent<Text>();
        ui_myDeaths = GameObject.Find("HUD/Death/Text").GetComponent<Text>();
        ui_leaderboard = GameObject.Find("HUD").transform.Find("Leaderboard").transform;
        ui_endGame = GameObject.Find("EndGame").transform;
        ui_timer = GameObject.Find("HUD/Timer/Text").GetComponent<Text>();

        RefreshMyStats();
    }

    private void SetActiveToFalse()
    {
        ui_leaderboard.gameObject.SetActive(false);
        ui_endGame.gameObject.SetActive(false);
    }

    private void RefreshMyStats()
    {
        if (playerInfo.Count > myId)
        {
            ui_mykills.text = $"Kills: {playerInfo[myId].kills}";
            ui_myDeaths.text = $"Deaths: {playerInfo[myId].deaths}";
        }
        else
        {
            ui_mykills.text = $"Kills: 0";
            ui_myDeaths.text = $"Deaths: 0";
        } 
    }

    private void InitMatchTimer()
    {
        currentTime = matchLength;
        RefreshMatchTimer();

        if (PhotonNetwork.IsMasterClient)
            timerCoroutine = StartCoroutine(timer());
    }

    private void RefreshMatchTimer()
    {
        

        string min = (currentTime / 60).ToString("00");
        string sec = (currentTime % 60).ToString("00");

        ui_timer.text = min + ":" + sec;

    }

    public void Spawn()
    {
        //Get a random spawn point
        Transform t_spawn = spawns[Random.Range(0, spawns.Length)];
        //Instantiate player at that point
        PhotonNetwork.Instantiate(player_prefab_string, t_spawn.position, t_spawn.rotation);
    }

    private void LeaderBoard(Transform t_lb)
    {
        //Delete any unneeded object on leaderboard
        for (int i = 3; i < t_lb.childCount; i++)
        {
            Destroy(t_lb.GetChild(i).gameObject);
        }

        //Set lobby details
        t_lb.Find("Header/Title").GetComponent<TMP_Text>().text = System.Enum.GetName(typeof(GameMode), GameSettings.gameMode);
        //t_lb.Find("Header/Map").GetComponent<TMP_Text>().text = SceneManager.GetActiveScene().name;
        t_lb.Find("Header/Map").GetComponent<TMP_Text>().text = "MEH";


        //save playercard object
        GameObject playercard = t_lb.GetChild(2).gameObject; //The playercard will always start at 2
        playercard.SetActive(false);

        //Sort the leaderboard
        List<PlayerInfo> sorted = SortPlayer(playerInfo);

        //display the playercards
        foreach (PlayerInfo p in sorted)
        {
            GameObject newCard = Instantiate(playercard, t_lb);

            //Change Username
            newCard.transform.Find("username").gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = p.profile.username;
            //Change kills
            newCard.transform.Find("Kills").gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = p.kills.ToString();
            //Changes deaths
            newCard.transform.Find("Deaths").gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = p.deaths.ToString();
            //Change gold
            newCard.transform.Find("Gold").gameObject.transform.GetChild(0).GetComponent<TMP_Text>().text = "500";

            newCard.SetActive(true);
        }

        //Activate leaderboard
        t_lb.gameObject.SetActive(true);
    }

    private List<PlayerInfo> SortPlayer(List<PlayerInfo> t_list)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while(sorted.Count < t_list.Count)
        {
            //set default
            short highest = -1;
            PlayerInfo selection = t_list[0];
            foreach(PlayerInfo a in t_list)
            {
                if (sorted.Contains(a)) continue;
                if(a.kills > highest)
                {
                    selection = a;
                    highest = a.kills;
                }
            }

            //add Player
            sorted.Add(selection);
        }

        return sorted;
    }


    #region Photon
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;

        EventCodes ec = (EventCodes)photonEvent.Code;

        Debug.Log("EVENT CODE: " +ec);

        object[] obj = (object[])photonEvent.CustomData;

        switch (ec)
        {
            case EventCodes.NewPlayer:
                NewPlayer_R(obj);
                break;
            case EventCodes.UpdatePlayers:
                UpdatePlayers_R(obj);
                break;
            case EventCodes.ChangeStat:
                ChangeStat_R(obj);
                break;
            case EventCodes.RefreshTimer:
                RefreshMatchTimer_R(obj);
                break;
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        SceneManager.LoadScene(mainMenu);
    }
    #endregion

    private void ValidateConnection()
    {
        if (PhotonNetwork.IsConnected) return; //Able to connet go to room
        else
            SceneManager.LoadScene(mainMenu); //else return back to main menu
    }

    private void StateCheck()
    {
        if(state == GameState.Ending)
        {
            EndGame();
        }
    }

    private void ScoreCheck()
    {
        //Check if winning condition is met
        bool detectWin = false;

        foreach(PlayerInfo p in playerInfo)
        {
            if(p.kills == killCount)
            {
                detectWin = true;
                break;
            }
        }

        //Found winner
        if (detectWin)
        {
            //Are we the host ? and the game is still going on
            if(PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                //Tell everyone that the game is over
                UpdatePlayers_S((int)GameState.Ending, playerInfo);
            }
        }

    }

    private void EndGame()
    {
        //End the game
        state = GameState.Ending;

        //Stop match timer routine
        if (timerCoroutine != null) StopCoroutine(timer());
        currentTime = 0;
        RefreshMatchTimer();

        //Only the host/master client can do this now
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

        //Show map camera
        mapCam.SetActive(true);

        //Show Leaderboard
        ui_endGame.gameObject.SetActive(true);
        LeaderBoard(ui_endGame.Find("Leaderboard"));

        //Wait a bit before returning
        StartCoroutine(End(6f));
    }

    #region Coroutine
    IEnumerator End(float time)
    {
        yield return new WaitForSeconds(time);

        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();

    }

    IEnumerator timer()
    {
        yield return new WaitForSeconds(1);

        currentTime -= 1;

        //Check if timer is finished
        if (currentTime <= 0)
        {
            //game is finished
            timerCoroutine = null;
            UpdatePlayers_S((int)GameState.Ending, playerInfo);
        }
        else
        {
            RefreshMatchTimer_S();
            timerCoroutine = StartCoroutine(timer());
        }
    }

    private bool CalculateTeam()
    {
        return false;
    }

    #endregion

    #region Event

    public void NewPlayer_S(PlayerData t_data)
    {
        Debug.Log("SENDING NEW PLAYER DATA");
        object[] package = new object[numOfFields];

        //Set data for new player
        package[0] = t_data.username;
        package[1] = t_data.level;
        package[2] = t_data.xp;
        package[3] = PhotonNetwork.LocalPlayer.ActorNumber;
        //This is where we can set the base number such as gold, and KDA for new players
        package[4] = (short)0;
        package[5] = (short)0;
        package[6] = CalculateTeam();

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void NewPlayer_R(object[] t_data)
    {
        Debug.Log("RECIEVING NEW PLAYER DATA");
        PlayerInfo p = new PlayerInfo(
            new PlayerData((string)t_data[0], (int)t_data[1], (int)t_data[2]),
            (int)t_data[3],
            (short)t_data[4],
            (short)t_data[5],
            (bool)t_data[6]
        );

        playerInfo.Add(p);

        UpdatePlayers_S((int)state, playerInfo);
    }

    public void UpdatePlayers_S(int state, List<PlayerInfo> info)
    {
        Debug.Log("SENDING UPDATE PLAYER DATA");
        //Create package array the size of how many players there are
        //in the room
        object[] package = new object[info.Count + 1];

        //This keeps track the state of the game for all players
        package[0] = state;

        for(int i = 0; i < info.Count; i++)
        {
            object[] temp_obj = new object[numOfFields];

             temp_obj[0] = info[i].profile.username;
             temp_obj[1] = info[i].profile.level;
             temp_obj[2] = info[i].profile.xp;
             temp_obj[3] = info[i].actor;
             temp_obj[4] = info[i].kills;
             temp_obj[5] = info[i].deaths;
             temp_obj[6] = info[i].awayTeam;

            package[i + 1] = temp_obj;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdatePlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void UpdatePlayers_R(object[] data)
    {
        state = (GameState)data[0];

        Debug.Log("RECIEVING UPDATE PLAYER DATA");
        playerInfo = new List<PlayerInfo>();

        //Start at 1 since 0 is Game State
        for(int i = 1; i < data.Length; i++)
        {
            object[] temp_data = (object[])data[i];

            PlayerInfo p = new PlayerInfo(
                new PlayerData(
                    (string)temp_data[0],
                    (int)temp_data[1],
                    (int)temp_data[2]
                    ),
                (int)temp_data[3],
                (short)temp_data[4],
                (short)temp_data[5],
                (bool)temp_data[6]
                );

            playerInfo.Add(p);

            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor) myId = i - 1;
        }
        //Now check if anyone meets winning condition
        StateCheck();
    }

    public void ChangeStat_S(int actor, byte stat, byte amt)
    {
        Debug.Log("SENDING CHANGE PLAYER DATA");
        object[] package = new object[] { actor, stat, amt };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ChangeStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void ChangeStat_R(object[] data)
    {
        Debug.Log("RECIEVING NEW PLAYER DATA");
        int act = (int)data[0];
        byte stat = (byte)data[1];
        byte amt = (byte)data[2];

        for(int i = 0; i < playerInfo.Count; i++)
        {
            if (playerInfo[i].actor == act)
            {
                switch (stat) 
                {
                    case 0: //Kills
                        playerInfo[i].kills += amt;
                        Debug.Log($"Player {playerInfo[i].profile.username} : Kills = {playerInfo[i].kills}");
                        break;
                    case 1: //Death
                        playerInfo[i].deaths += amt;
                        Debug.Log($"Player {playerInfo[i].profile.username} : Deaths = {playerInfo[i].deaths}");
                        break;
                }

                if (i == myId) RefreshMyStats();
                if (ui_leaderboard.gameObject.activeSelf) LeaderBoard(ui_leaderboard);
            }
        }
        //Check if winning condition is met
        ScoreCheck();
    }

    public void RefreshMatchTimer_S()
    {
        object[] package = new object[] { currentTime };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.RefreshTimer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }
    public void RefreshMatchTimer_R(object[] data)
    {
        int t_time = (int)data[0];

        RefreshMatchTimer();
    }

    #endregion
}
