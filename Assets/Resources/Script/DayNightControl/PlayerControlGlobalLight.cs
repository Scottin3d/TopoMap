using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerControlGlobalLight : MonoBehaviour
{
    //X represent east west, Light rotate at Z direction
    private Quaternion LightRotation;
    //private static GameObject MyDirectionalLight;
    public Slider LightSlider;
    public Light MyDirectionalSunLight;
    public Gradient SunColor;

    void Awake()
    {
        //MyDirectionalLight = this.GetComponent<Light>();
    }

    void Start()
    {
        LightRotation = Quaternion.Euler(90, 0, 0);
    }

    void Update()
    {
    }

    //Change direction of the light for all the user.
    public void SetLight()
    {
        LightRotation = Quaternion.Euler(LightSlider.value, 0, 0);
        MyDirectionalSunLight.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            MyDirectionalSunLight.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(LightRotation);
        });

        SunSinFunction();
    }

    private void SunSinFunction()
    {
        //Change light intensity
        float SunRadian = MyDirectionalSunLight.transform.eulerAngles.x * Mathf.PI / 180;
        if (Mathf.Sin(SunRadian) >= 0)
        {
            MyDirectionalSunLight.intensity = Mathf.Sin(SunRadian);
        }
        else
        {
            MyDirectionalSunLight.intensity = 0;
        }
        //Change light color
        MyDirectionalSunLight.color = SunColor.Evaluate(MyDirectionalSunLight.intensity);
    }
}
