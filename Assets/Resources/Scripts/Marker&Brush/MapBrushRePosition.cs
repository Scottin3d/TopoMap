using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBrushRePosition : MonoBehaviour
{
    public Vector3 SecondToLastBrushPosition = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        RaycastHit Hit;
        if (Physics.Raycast(this.transform.position, Vector3.down, out Hit))
        {
            //if (Hit.collider.tag == "Chunk")
            //{
            //    Vector3 NewPosition = new Vector3(this.transform.position.x, Hit.point.y - 2.2f, this.transform.position.z);
            //    this.transform.position = NewPosition;
            //    this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            //    {
            //        this.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(NewPosition);
            //    });
            //}
            Vector3 NewPosition = new Vector3(this.transform.position.x, Hit.point.y - 2.2f, this.transform.position.z);
            this.transform.position = NewPosition;
            this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                this.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(NewPosition);
            });
        }
    }
}
