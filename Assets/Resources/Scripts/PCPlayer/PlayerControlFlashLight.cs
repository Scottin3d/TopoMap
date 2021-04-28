using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlFlashLight : MonoBehaviour
{
    private Camera PlayerCamera;
    private static GameObject MyFlashLight;
    private bool IfOn = false;
    private static bool StartUpdate = false;

    void Awake()
    {
        PlayerCamera = GameObject.Find("PCHandler/Player").GetComponentInChildren<Camera>();
    }

    void Start()
    {
        ASL.ASLHelper.InstantiateASLObject("PlayerFlashLight", new Vector3(0, 60, 0), Quaternion.identity, "", "", GetLightObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (StartUpdate)
        {
            UpdateFlashLightPositionAndRotation();
            ControlFlashLight();
        }
    }

    private void ControlFlashLight()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (IfOn)
            {
                MyFlashLight.SetActive(false);
                IfOn = false;
            }
            else
            {
                MyFlashLight.SetActive(true);
                IfOn = true;
            }
        }
    }

    private void UpdateFlashLightPositionAndRotation()
    {
        if (MyFlashLight.activeSelf == false)
        {
            return;
        }
        MyFlashLight.transform.position = PlayerCamera.transform.position;
        MyFlashLight.transform.rotation = PlayerCamera.transform.rotation;

        MyFlashLight.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            MyFlashLight.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(PlayerCamera.transform.rotation);
            MyFlashLight.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(PlayerCamera.transform.position);
        });
    }

    private static void GetLightObject(GameObject _myGameObject)
    {
        MyFlashLight = _myGameObject;
        MyFlashLight.SetActive(false);
        StartUpdate = true;
    }
}
