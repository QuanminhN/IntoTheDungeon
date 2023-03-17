using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public string player_prefab;
    public Transform[] spawns;
    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        //Get a random spawn point
        Transform t_spawn = spawns[Random.Range(0, spawns.Length)];
        //Instantiate player at that point
        PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
    }
}
