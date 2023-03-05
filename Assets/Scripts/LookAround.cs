using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAround : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] private Transform player;

    [SerializeField] private float xSensitivity = 100f;
    [SerializeField] private float ySensitivity = 100f;
    [SerializeField] private float maxAngle = 90f;
    private float xRotation;

    // Start is called before the first frame update
    void Start()
    {
        updateCursorState();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -maxAngle, maxAngle);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            updateCursorState();
        }
    }

    void updateCursorState()
    {
        Cursor.visible = !Cursor.visible;
        if (!Cursor.visible)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
        
    }

}
