using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBrushRePosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Called");
        if (other.gameObject.tag == "Chunk")
        {
            Debug.Log("Called");
            float YScale = this.transform.localScale.y / 2;
            Vector3 NewPosition = new Vector3(other.transform.position.x, other.transform.position.y - YScale + 2f, other.transform.position.z);

            this.transform.position = NewPosition;
            this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                this.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(NewPosition);
            });

            this.GetComponent<BoxCollider>().enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Called");
        if (other.gameObject.tag == "Chunk")
        {
            Debug.Log("CalledEnter");
            float YScale = this.transform.localScale.y / 2;
            Vector3 NewPosition = new Vector3(other.transform.position.x, other.transform.position.y - YScale + 2f, other.transform.position.z);

            this.transform.position = NewPosition;
            this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                this.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(NewPosition);
            });

            this.GetComponent<BoxCollider>().enabled = false;
        }
    }
}
