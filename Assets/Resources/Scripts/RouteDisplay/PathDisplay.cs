using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public static class PathDisplay //: MonoBehaviour
{
    
    private static List<SplineWalker> walkers = new List<SplineWalker>();
    private static int walkerCount = 0;

    private static GameObject cam;
    private static Vector3 camOffset = new Vector3(0f, 2f, 0f);
    private static Quaternion camRot = new Quaternion(-0.2f, 0f, 0f, 1f);


    private static bool InitFinished = false;
    private static bool PoolFinished = false;

    //Variables from RouteDisplay
    private static Color myColor;
    private static BezierSpline mySpline;
    private static float updatesPerSecond = 10f;
    private static float speed = 0.5f;

    public static IEnumerator Initialize(Color _c, float tSpeed, float ups)
    {
        cam = GameObject.Find("PathCam");
        Debug.Assert(cam != null, "Missing path camera");
        DeactivatePathCam();

        myColor = _c;
        updatesPerSecond = ups;
        speed = tSpeed;
        yield return new WaitForSeconds(1f);
        InitFinished = true;
    }

    public static IEnumerator GeneratePathPool(int amount)
    {
        while (!InitFinished) yield return new WaitForSeconds(0.1f);
        int ndx = 0;
        while(ndx < amount)
        {
            yield return new WaitForSeconds(1f / updatesPerSecond);
            ASLHelper.InstantiateASLObject("PathWalker", new Vector3(0, 0, 0), Quaternion.identity, "", "", PathTraceInstantiation);
            ndx++;
            walkerCount++;
        }
        yield return new WaitForSeconds(1f);
        PoolFinished = true;
    }

    public static IEnumerator StartTrace()
    {
        if(cam.transform.parent == null) cam.SetActive(false);
        while (!PoolFinished) yield return new WaitForSeconds(0.1f);
        float maxDuration = (mySpline != null) ? mySpline.CurveCount : 0;
        float spacing = (maxDuration > 0) ? maxDuration / walkerCount : 0;
        float curDuration = 0f;

        for(int ndx = 0; ndx < walkerCount; ndx++)
        {
            walkers[ndx].Reset();
            walkers[ndx].spline = mySpline;
            walkers[ndx].duration = maxDuration;
            curDuration += spacing * speed;
            walkers[ndx].Begin(curDuration < maxDuration, (float)ndx / (float)walkerCount);
            //yield return new WaitForSeconds(spacing);
        }
        yield return new WaitForSeconds(0.1f);
    }

    public static IEnumerator Render()
    {
        while (true)
        {
            cam.transform.localRotation = camRot;
            cam.transform.localPosition = camOffset;
            foreach(SplineWalker _s in walkers)
            {
                _s.gameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    _s.gameObject.GetComponent<ASLObject>().SendAndSetLocalPosition(_s.gameObject.transform.position);
                    _s.gameObject.GetComponent<ASLObject>().SendAndSetLocalRotation(_s.gameObject.transform.localRotation);
                });
            }
            yield return new WaitForSeconds(1f / updatesPerSecond);
        }
    }

    public static void SetSpline(BezierSpline _b)
    {
        mySpline = _b;
        PathDisplayHelper.Instance.StartCoroutine(StartTrace());
    }

    public static void Select(Transform _t)
    {
        if(cam.transform.parent != null && _t != cam.transform.parent)
        {
            if(_t.gameObject.layer != 9) cam.transform.parent.DetachChildren();
        }
        if (_t.gameObject.layer == 10)
        {
            cam.SetActive(true);
            if (cam.transform.parent != null) cam.transform.parent.DetachChildren();
            cam.transform.SetParent(_t, false);
        }
        else if(_t.gameObject.layer != 9 )
        {
            Debug.Log("detatching path cam...");
            DeactivatePathCam();
        }
    }

    public static void Detatch()
    {
        if(cam.transform.parent != null) cam.transform.parent.DetachChildren();
        DeactivatePathCam();
    }

    private static void DeactivatePathCam()
    {
        cam.SetActive(false);

        GameObject _pathTex = GameObject.FindWithTag("PathCam");
        RawImage raw = (_pathTex != null) ? _pathTex.GetComponent<RawImage>() : null;
        RenderTexture _render = (raw != null) ? raw.texture as RenderTexture : null;
        
        if (_render != null)
        {
            //From https://forum.unity.com/threads/how-to-clear-a-render-texture-to-transparent-color-all-bytes-at-0.147431/
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = _render;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt; 
        }
    }

    private static void PathTraceInstantiation(GameObject _myGameObject)
    {
        if (_myGameObject.GetComponent<SplineWalker>() != null)
        {
            walkers.Add(_myGameObject.GetComponent<SplineWalker>());
            _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                _myGameObject.GetComponent<ASLObject>().SendAndSetObjectColor(myColor, myColor);
            });
            _myGameObject.GetComponent<MeshRenderer>().enabled = false;
            _myGameObject.GetComponent<MeshCollider>().enabled = false;
        }
        else
        {
            _myGameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                _myGameObject.GetComponent<ASLObject>().DeleteObject();
            });
        }
    }
}

//From https://www.reddit.com/r/Unity3D/comments/3y2scl/how_to_call_a_coroutine_from_a/
//Used in the event we need to call a coroutine from inside PathDisplay
public class PathDisplayHelper : MonoBehaviour
{
    private static PathDisplayHelper _Instance;

    public static PathDisplayHelper Instance
    {
        get
        {
            if (_Instance == null) _Instance = new GameObject("PathDisplayHelper").AddComponent<PathDisplayHelper>();
            return _Instance;
        }
    }
}
