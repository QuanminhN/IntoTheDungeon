using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public string player_prefab;
    public Transform spawn;
    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate(player_prefab, spawn.position, spawn.rotation);
    }
}
