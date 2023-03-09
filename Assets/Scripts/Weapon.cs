using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Gun[] loadout;
    public Transform weaponParent;

    private GameObject currentEquip;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
    }

    void Equip(int i)
    {
        //This will destroy weapson if player already have a weapon equiped
        if (currentEquip != null) Destroy(currentEquip);

        GameObject t_newEquipment = Instantiate(loadout[i].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newEquipment.transform.localPosition = Vector3.zero;
        t_newEquipment.transform.localEulerAngles = Vector3.zero;

        currentEquip = t_newEquipment;
    }
}
