using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlICameraWaitingRoom : MonoBehaviour
{
    private float MouseSpeed;
    public Transform PlayerObject;
    private float XMove;

    // Start is called before the first frame update
    void Start()
    {
        MouseSpeed = 100;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeCameraDirection();
    }

    //private void FixedUpdate()
    //{
    //    ChangeCameraDirection();
    //}

    private void ChangeCameraDirection()
    {
        float XDirection, YDirection;
        XDirection = Input.GetAxisRaw("Mouse X") * MouseSpeed * Time.fixedDeltaTime;
        YDirection = Input.GetAxisRaw("Mouse Y") * MouseSpeed * Time.fixedDeltaTime;
        XMove = XMove - YDirection;
        XMove = Mathf.Clamp(XMove, -90, 90);
        this.transform.localRotation = Quaternion.Euler(XMove, 0, 0);

        PlayerObject.Rotate(Vector3.up * XDirection);
    }
}
