using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Occluder : MonoBehaviour
{
    public static bool EnableZone;
    public GameObject Zone;

    private void Start() {
        Zone.SetActive(false);
    }

    public void disableZone() {
        if(Zone.activeSelf == true) {
            Zone.SetActive(false);
        }
    }

    public void enableZone() {
        if (Zone.activeSelf == false) {
            Zone.SetActive(true);
        }
    }

   
}
