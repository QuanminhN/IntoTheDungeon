using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Gun", menuName ="Gun")]
public class Gun : ScriptableObject
{
    public string gunName;
    public float fireRate;
    public GameObject prefab;

    public float aimSpeed;

    public float bloom; // accuracy
    public float recoil;
    public float kickback;
}
