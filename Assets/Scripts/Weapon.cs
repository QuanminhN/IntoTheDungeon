using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Variables
    public Gun[] loadout;
    public Transform weaponParent;

    private int currentIndex;
    private GameObject currentEquip;

    public GameObject bulletHolePrefab;
    public LayerMask canBeShot;
    #endregion

    #region Monobehavior callbacks
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
        if(currentEquip != null)
        {
            Aim(Input.GetMouseButton(1));

            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }
        
    }
    #endregion

    #region Private Methods
    void Equip(int i)
    {
        //This will destroy weapson if player already have a weapon equiped
        if (currentEquip != null) Destroy(currentEquip);

        currentIndex = i;

        GameObject t_newEquipment = Instantiate(loadout[i].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newEquipment.transform.localPosition = Vector3.zero;
        t_newEquipment.transform.localEulerAngles = Vector3.zero;

        currentEquip = t_newEquipment;
    }

    void Aim(bool isAiming)
    {

        Transform t_anchor = currentEquip.transform.Find("Anchor");
        Transform t_AdsState = currentEquip.transform.Find("States/ADS");
        Transform t_HipState = currentEquip.transform.Find("States/Hip");


        if (isAiming)
        {
            //Aim
            t_anchor.position = Vector3.Lerp(t_anchor.position, t_AdsState.position, loadout[currentIndex].aimSpeed * Time.deltaTime);
        }
        else
        {
            //Hip
            t_anchor.position = Vector3.Lerp(t_anchor.position, t_HipState.position, loadout[currentIndex].aimSpeed * Time.deltaTime);
        }

    }

    void Shoot()
    {
        Transform t_spawn = transform.Find("Cameras/Player Camera");

        RaycastHit t_hit = new RaycastHit();
        if(Physics.Raycast(t_spawn.position, t_spawn.forward, out t_hit, 1000f, canBeShot)) //start from camera, ahead of camera, distance of raycast, and layermask of what can be shot
        {
            //Create bullethole object where it is hit and slightly off the wall
            GameObject t_newBulletHole = Instantiate(bulletHolePrefab, t_hit.point + t_hit.normal * .001f, Quaternion.identity) as GameObject;
            t_newBulletHole.transform.LookAt(t_hit.point + t_hit.normal);
            Destroy(t_newBulletHole, 5f); //Destory in 5 seconds
        }
    }
    #endregion
}
