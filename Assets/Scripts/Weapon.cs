using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Weapon : MonoBehaviourPunCallbacks
{
    #region Variables
    public Gun[] loadout;
    public Transform weaponParent;

    private int currentIndex;
    private GameObject currentEquip;

    public GameObject bulletHolePrefab;
    public LayerMask canBeShot;

    private float shotCoolDown;

    private bool isReloading;

    public bool isAiming = false;

    [HideInInspector] public Gun currentGunData;
    #endregion

    #region Monobehavior callbacks
    // Start is called before the first frame update
    void Start()
    {
        foreach(Gun gun in loadout)
        {
            gun.initGun();
        }
        Equip(0);
    }

    // Update is called once per frame
    void Update()
    {

        if (Pause.paused && photonView.IsMine) return; //

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            photonView.RPC("Equip", RpcTarget.All, 0);
        }
        else if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
        {
            photonView.RPC("Equip", RpcTarget.All, 1);
        }
        else if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha3))
        {
            photonView.RPC("Equip", RpcTarget.All, 2);
        }
        if (currentEquip != null)
        {
            if (photonView.IsMine)
            {
                //Aim(Input.GetMouseButton(1));

                if (loadout[currentIndex].burstMode != 1) // Burst or Semi
                {
                    if (Input.GetMouseButtonDown(0) && shotCoolDown <= 0 && !isReloading)
                    {
                        if (loadout[currentIndex].canFireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                        else
                        {
                            StartCoroutine(Reload(loadout[currentIndex].reloadTimer));
                        }
                    }
                }
                else
                {
                    if (Input.GetMouseButton(0) && shotCoolDown < 0 && !isReloading) //Full auto
                    {
                        if (loadout[currentIndex].canFireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                        else
                        {
                            StartCoroutine(Reload(loadout[currentIndex].reloadTimer));
                        }
                    }

                }


                if (loadout[currentIndex].clip != loadout[currentIndex].clipsize)
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        StartCoroutine(Reload(loadout[currentIndex].reloadTimer));
                    }
                }
                

                shotCoolDown -= Time.deltaTime;
            }
            //weapon position elasticity
            currentEquip.transform.localPosition = Vector3.Lerp(currentEquip.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            currentEquip.transform.rotation = Quaternion.Lerp(currentEquip.transform.rotation, weaponParent.rotation, Time.deltaTime * 4f);
        }
        
    }
    #endregion

    #region Private Methods
    [PunRPC]
    void Equip(int i)
    {
        //This will destroy weapson if player already have a weapon equiped
        if (currentEquip != null)
        {
            if(isReloading) StopCoroutine("Reload"); //Stop reloading if swap weapon
            Destroy(currentEquip);
        }

        currentIndex = i;

        GameObject t_newEquipment = Instantiate(loadout[i].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newEquipment.GetComponent<Rigidbody>().isKinematic = true;
        t_newEquipment.transform.localPosition = Vector3.zero;
        t_newEquipment.transform.localEulerAngles = Vector3.zero;
        t_newEquipment.GetComponent<Sway>().isMine = photonView.IsMine;

        if (photonView.IsMine) ChangeLayers(t_newEquipment, 8); //This number is the layer for Gun
        else ChangeLayers(t_newEquipment, 0);

        currentEquip = t_newEquipment;
        currentGunData = loadout[i];
    }

    private void ChangeLayers(GameObject p_obj, int p_layer)
    {
        p_obj.layer = p_layer;
        foreach(Transform a in p_obj.transform)
        {
            ChangeLayers(a.gameObject, p_layer);
        }
    }

    public void Aim(bool isAiming)
    {
        if (!currentEquip) return;

        this.isAiming = isAiming;
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

    [PunRPC]
    void Shoot()
    {
        Transform t_spawn = transform.Find("Cameras/Player Camera");
        //Set up Bloom
        for(int i = 0; i < loadout[currentIndex].pellets; i++)
        {
            Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
            //calculate Bloom
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();
            //Set shooting cooldown
            shotCoolDown = loadout[currentIndex].fireRate;

            //Raycast
            RaycastHit t_hit = new RaycastHit();
            if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot)) //start from camera, ahead of camera, distance of raycast, and layermask of what can be shot
            {
                //Create bullethole object if its not on a player
                if (t_hit.transform.gameObject.layer != 9)
                {
                    GameObject t_newBulletHole = Instantiate(bulletHolePrefab, t_hit.point + t_hit.normal * .001f, Quaternion.identity) as GameObject;
                    t_newBulletHole.transform.LookAt(t_hit.point + t_hit.normal);
                    Destroy(t_newBulletHole, 5f); //Destory in 5 seconds
                }

                if (photonView.IsMine)
                {
                    //Shooing other players
                    if (t_hit.collider.gameObject.layer == 9)
                    {
                        t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage);
                    }
                }
            }
        }
        

       

        //gun fx
        currentEquip.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
        currentEquip.transform.position -= currentEquip.transform.forward * loadout[currentIndex].kickback;

        
    }
    [PunRPC]
    void TakeDamage(int dmg)
    {
        GetComponent<PlayerMovement>().TakeDamage(dmg);
    }

    IEnumerator Reload(float wait)
    {
        isReloading = true;
        if (currentEquip.GetComponent<Animator>())
        {
            currentEquip.GetComponent<Animator>().Play("Reload", 0, 0);
        }
        else
        {
            currentEquip.SetActive(false);
        }

        yield return new WaitForSeconds(wait);

        loadout[currentIndex].Reload();
        currentEquip.SetActive(true);
        isReloading = false;
    }
    #endregion
    #region Public Method
    public void refreshAmmo(Text p_text) 
    {
        int t_clip = loadout[currentIndex].getClip();
        int t_stash = loadout[currentIndex].getStash();

        p_text.text = t_clip.ToString() + " / " + t_stash.ToString();
    }
    #endregion
}
