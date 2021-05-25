using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCompass : MonoBehaviour
{
    //Negative Z represent East
    //Positive Y represent South

    public GameObject PC_Player;
    private Vector3 CompassVector;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CompassVector.z = PC_Player.transform.eulerAngles.y;
        this.transform.localEulerAngles = CompassVector;
    }
}
