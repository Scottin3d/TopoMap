using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//For testing purposes. Write desired functionality and attach it to a game object

public class TestingScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            MinimapDisplay.AddRouteMarker(transform.position);
        }
    }
}
