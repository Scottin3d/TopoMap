using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToObject : MonoBehaviour
{
    public GameObject TargetObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetObject != null)
        {
            this.transform.LookAt(TargetObject.transform);

            //3D Text object will face to the backward.
            if (this.transform.tag == "3DText")
            {
                this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y + 180, this.transform.rotation.eulerAngles.z);
            }
        }
    }
}
