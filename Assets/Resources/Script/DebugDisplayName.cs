using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDisplayName : MonoBehaviour
{

    //attatch this class to an object and it will display it's string name on start at runtime.
    //handy for if you need to know the name of an object you know exists but can't reach in code.


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(this.gameObject.name);
    }
}
