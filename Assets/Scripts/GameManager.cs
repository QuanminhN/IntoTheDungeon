using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerInfo
{
    //Keep data of player in the match
    public PlayerData profile;
    public int actor;
    public short kills;
    public short deaths;

    public PlayerInfo(PlayerData t_profile, int t_act, short t_kills, short t_deaths)
    {
        profile = t_profile;
        actor = t_act;
        kills = t_kills;
        deaths = t_deaths;
    }
}

public class GameManager : MonoBehaviour, IOnEventCallback
{
    #region Fields
    public GameObject player_prefab;
    public string player_prefab_string;
    public Transform[] spawns;

    public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
    public int myId;

    private int numOfFields = 6;
    #endregion

    #region Enums
    public enum EventCodes : byte 
    { 
        NewPlayer,
        UpdatePlayers,
        ChangeStat,
    }

    #endregion
    private void Start()
    {
        ValidateConnection();
        NewPlayer_S(Launcher.myProfile);
        Spawn();
    }

    public void Spawn()
    {
        //Get a random spawn point
        Transform t_spawn = spawns[Random.Range(0, spawns.Length)];
        //Instantiate player at that point
        PhotonNetwork.Instantiate(player_prefab_string, t_spawn.position, t_spawn.rotation);
    }

    #region Photon
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;

        EventCodes ec = (EventCodes)photonEvent.Code;

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
        }


    }
    #endregion

    private void ValidateConnection()
    {
        if (PhotonNetwork.IsConnected) return; //Able to connet go to room
        SceneManager.LoadScene(0); //else return back to main menu
    }

    public void NewPlayer_S(PlayerData t_data)
    {
        object[] package = new object[numOfFields];

        //Set data for new player
        package[0] = t_data.username;
        package[1] = t_data.level;
        package[2] = t_data.xp;
        package[3] = PhotonNetwork.LocalPlayer.ActorNumber;
        //This is where we can set the base number such as gold, and KDA for new players
        package[4] = (short)0;
        package[5] = (short)0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void NewPlayer_R(object[] t_data)
    {
        PlayerInfo p = new PlayerInfo(
            new PlayerData((string)t_data[0], (int)t_data[1], (int)t_data[2]),
            (int)t_data[3],
            (short)t_data[4],
            (short)t_data[5]
            );
        playerInfo.Add(p);

        UpdatePlayers_S(playerInfo);
    }

    public void UpdatePlayers_S(List<PlayerInfo> info)
    {
        object[] package = new object[info.Count];

        for(int i = 0; i < info.Count; i++)
        {
            object[] temp_obj = new object[numOfFields];

             temp_obj[0] = info[i].profile.username;
             temp_obj[1] = info[i].profile.level;
             temp_obj[2] = info[i].profile.xp;
             temp_obj[3] = info[i].actor;
             temp_obj[4] = info[i].kills;
             temp_obj[5] = info[i].deaths;

            package[i] = temp_obj;
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
        playerInfo = new List<PlayerInfo>();

        for(int i = 0; i < data.Length; i++)
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
                (short)temp_data[5]
                );

            playerInfo.Add(p);

            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor) myId = i;
        }
    }

    public void ChangeStat_S(int actor, byte stat, byte amt)
    {
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

            }
        }
    }
}
