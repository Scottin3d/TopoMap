using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LocalSlider : MonoBehaviour
{
    public Slider slider = null;
    public Text sliderValue = null;

    // Start is called before the first frame update
    void Start()
    {
        sliderValue.text = slider.value.ToString();
    }

    public void UpdateLabel(float value) { 
        sliderValue.text = value.ToString();
    }

}
