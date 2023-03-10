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
        Aim(Input.GetMouseButton(1));
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
        if (currentEquip == null) return;

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
    #endregion
}
