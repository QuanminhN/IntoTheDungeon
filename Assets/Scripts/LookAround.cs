using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LookAround : MonoBehaviourPunCallbacks
{

    #region Variables
    [SerializeField] Camera cam;
    [SerializeField] private Transform player;
    public Transform weapon;


    [SerializeField] private float xSensitivity = 100f;
    [SerializeField] private float ySensitivity = 100f;
    [SerializeField] private float maxAngle = 90f;
    private float xRotation;
    #endregion

    #region MonoBehavior Callbacks
    // Start is called before the first frame update
    void Start()
    {
        updateCursorState();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Pause.paused) return;
        float mouseX = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -maxAngle, maxAngle);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        weapon.localRotation = cam.transform.localRotation;
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            updateCursorState();
        }
    }
    #endregion

    #region Private methods
    void updateCursorState()
    {
        Cursor.visible = !Cursor.visible;
        if (!Cursor.visible)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

    }
    #endregion

}
