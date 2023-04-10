using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enum for game modes
public enum GameMode
{
    FFA = 0,
    TDM = 1,
}

public class GameSettings : MonoBehaviour
{
    public static GameMode gameMode = GameMode.FFA;
}

//This class will hold game setting
//Such as how long the match, win condition, map, etc