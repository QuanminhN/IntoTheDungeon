using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sway : MonoBehaviourPunCallbacks
{
    #region Variables
    public float swayIntensity;
    public float smoothness;

    private Quaternion origin_roation;

    public bool isMine;
    #endregion

    #region Monobehavior callbacks
    // Start is called before the first frame update
    void Start()
    {
        origin_roation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        updateSway();
    }
    #endregion

    #region Private Methods
    private void updateSway()
    {
        //controls
        float t_xmove = Input.GetAxis("Mouse X");
        float t_ymove = Input.GetAxis("Mouse Y");

        if (!isMine)
        {
            t_xmove = 0;
            t_ymove = 0;
        }

        //calculation
        Quaternion t_xadj = Quaternion.AngleAxis(-swayIntensity * t_xmove, Vector3.up);
        Quaternion t_yadj = Quaternion.AngleAxis(swayIntensity * t_ymove, Vector3.right);
        Quaternion target_rotation = t_xadj * t_yadj * origin_roation;

        //Rotate to calculation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smoothness);
    }
    #endregion
}
