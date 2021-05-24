using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleLine : MonoBehaviour
{
    public static ScaleLine _sl;
    public Canvas myCanvas;
    public RectTransform RenderPanel;

    private static int lookValue = 0;
    private static GenerateMapFromHeightMap lookingAt;

    void Awake()
    {
        if (_sl == null) _sl = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(myCanvas != null);
        Debug.Assert(RenderPanel != null);
    }

    // Update is called once per frame
    void Update()
    {
        if(lookValue > 0)
        {
            Debug.Log("Looking at: " + lookingAt.gameObject.name);
        }
    }

    public static void CheckDisplay(Camera _c, GameObject _sm, GameObject _lm)
    {
        /*Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(_c);
        Bounds camBound = new Bounds(_c.transform.position, Vector3.one);
        foreach(Plane _p in cameraPlanes)
        {
            camBound.Encapsulate(_p.ClosestPointOnPlane(_c.transform.position));
        }

        Ray Cray = new Ray(_c.transform.position, _c.transform.forward);
        Ray Lray = new Ray(_c.transform.position + Vector3.left, _c.transform.forward);
        Ray Rray = new Ray(_c.transform.position + Vector3.right, _c.transform.forward);
        RaycastHit Chit;
        RaycastHit Lhit;
        RaycastHit Rhit;

        int castValue = 0;
        if(Physics.Raycast(Cray, out Chit, 100f))
        {
            if((Chit.collider.transform.parent.tag == "SpawnSmallMap") || (Chit.collider.transform.parent.tag == "SpawnLargerMap"))
            {
                castValue++;
            }
        }
        if (Physics.Raycast(Lray, out Lhit, 100f))
        {
            if ((Lhit.collider.transform.parent.tag == "SpawnSmallMap") || (Lhit.collider.transform.parent.tag == "SpawnLargerMap"))
            {
                castValue += 2;
            }
        }
        if (Physics.Raycast(Rray, out Rhit, 100f))
        {
            if ((Rhit.collider.transform.parent.tag == "SpawnSmallMap") || (Rhit.collider.transform.parent.tag == "SpawnLargerMap"))
            {
                castValue += 4;
            }
        }
        lookValue = castValue;

        switch (castValue)
        {
            case 7:
            case 6:
            case 5:
            case 4: 
                lookingAt = Rhit.collider.transform.parent.gameObject.GetComponent<GenerateMapFromHeightMap>();
                break;
            case 3:
            case 2:
                lookingAt = Lhit.collider.transform.parent.gameObject.GetComponent<GenerateMapFromHeightMap>();
                break;
            case 1:
                lookingAt = Chit.collider.transform.parent.gameObject.GetComponent<GenerateMapFromHeightMap>();
                break;
            default:
                break;
        }*/

    }

    public static void CheckIfInView(Renderer _r)
    {

    }
}
