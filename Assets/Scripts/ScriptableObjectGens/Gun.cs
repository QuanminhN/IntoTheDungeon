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

    public int damage;

    public int ammo;
    public int clipsize;

    public int stash; //Current ammo count
    public int clip; //Current clip count

    public float reloadTimer;

    public int burstMode; // 0 semi | 1 auto | 2 burst

    public int pellets;

    public void initGun()
    {
        stash = ammo;
        clip = clipsize;
    }

    public bool canFireBullet()
    {
        if (clip > 0)
        {
            clip -= 1;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        stash += clip;
        clip = Mathf.Min(clipsize, stash);
        stash -= clip;
    }

    public bool canReload()
    {
        return stash >= 0 && clip != clipsize;
    }

    public int getStash()
    {
        return stash;
    }
    public int getClip()
    {
        return clip;
    }
}
