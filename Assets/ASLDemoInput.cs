using Aws.GameLift.Realtime.Command;
#if UNITY_ANDROID || UNITY_IOS
using Google.XR.ARCoreExtensions;
#endif
using ASL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ASLDemoInput : MonoBehaviour {

    public LocalSlider[] sliders;

    public static ASLObject selectedObj = null;

    [SerializeField]
    public static List<ASLObject> sceneObjects = new List<ASLObject>();

    private void Start() {
        // translate
        //sliders[0].GetComponent<Slider>().onValueChanged.AddListener(OnSliderTranslate);
        // rotate
        //sliders[1].GetComponent<Slider>().onValueChanged.AddListener(OnSliderRotate);
        // scale
        //sliders[2].GetComponent<Slider>().onValueChanged.AddListener(OnSliderScale);
    }

    public void OnSliderTranslate(float v) {
        if (selectedObj) {
            Vector3 pos = new Vector3(0, v, 0);
            selectedObj.SendAndSetLocalPosition(pos);
        }
    }

    public void OnSliderRotate(float v) {
        if (selectedObj) {
            Quaternion rot = Quaternion.identity;
            rot.eulerAngles = new Vector3(0, v, 0);
            selectedObj.SendAndSetLocalRotation(rot);
        }
    }

    public void OnSliderScale(float v) {
        if (selectedObj) {
            Vector3 pos = new Vector3(v, v, v);
            selectedObj.SendAndSetLocalScale(pos);
        }
    }

    // Update is called once per frame
    void Update() {
        if (selectedObj && !selectedObj.m_Mine) {
            selectedObj.SendAndSetObjectColor(Color.white, Color.white);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(ray, out hit)) {
                ASLObject previous = selectedObj;

                if (hit.collider.TryGetComponent<ASLObject>(out selectedObj)) {

                    selectedObj.SendAndSetClaim(() => {
                        previous.SendAndSetObjectColor(Color.white, Color.white);
                        Debug.Log("Successfully claimed object!");
                    });

                    if (selectedObj.m_Mine) {
                        selectedObj.SendAndSetObjectColor(Color.green, Color.red);
                    }
                    
                }
            }

        }

        // unselect
        if (Input.GetMouseButtonDown(1)) {
            if (selectedObj) {
                selectedObj.SendAndSetObjectColor(Color.white, Color.white);
                selectedObj = null;
            }
        }



        // create cube
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Cylinder,
                        new Vector3(Random.Range(-5f, 5f), Random.Range(0f, 2f), Random.Range(-2f, 2f)),
                        Quaternion.identity);

        }

        // create cube
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Cube,
                        new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 2f), Random.Range(-2f, 2f)),
                        Quaternion.identity);

        }

        // create cube
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Capsule,
                        new Vector3(Random.Range(-2f, 2f), Random.Range(0f, 2f), Random.Range(-2f, 2f)),
                        Quaternion.identity);

        }

        if (Input.GetKeyDown(KeyCode.D)) {
            selectedObj.SendAndSetClaim(() =>
            {
                sceneObjects.Remove(selectedObj);
                selectedObj.DeleteObject();
            });

            selectedObj = null;
        }
    }

    public static void WhatToDoWithMyGameObjectNowThatItIsCreated(GameObject _myGameObject) {
        sceneObjects.Add(_myGameObject.GetComponent<ASLObject>());

    }
}
