using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ASL;

public static class BrushGeneratorV2
{
    private static List<GameObject> smallBrush = new List<GameObject>();
    private static List<GameObject> largeBrush = new List<GameObject>();

    private static int fingerID = -1;
    private static int brushCount = 0;

    private static bool IsClearingBrush = false;

    #region DRAW

    public static void DrawBrush(Ray mouseRay, Vector3 _smCenter, Vector3 _lmCenter)
    {
        if (IsClearingBrush) return;
        RaycastHit hit;
        if(Physics.Raycast(mouseRay, out hit, 100f))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID)) return;
            
            if(hit.collider.transform.parent.tag == "SpawnSmallMap")
            {
                Vector3 largePos = (hit.point - _smCenter) * MarkerDisplay.GetScaleFactor() + _lmCenter;
                largePos += 3f * Vector3.up;
                ASLHelper.InstantiateASLObject("Brush", hit.point, Quaternion.identity, "", "", InstantiateSmallBrush);
                ASLHelper.InstantiateASLObject("LargeBrush", largePos, Quaternion.identity, "", "", InsantiateLargeBrush);
                brushCount++;
            }
        }
    }

    private static void InstantiateSmallBrush(GameObject _myBrush)
    {
        _myBrush.transform.parent = BrushStorage.Instance.gameObject.transform;
        smallBrush.Add(_myBrush);
    }

    private static void InsantiateLargeBrush(GameObject _myBrush)
    {
        _myBrush.transform.parent = BrushStorage.Instance.gameObject.transform;
        largeBrush.Add(_myBrush);
    }

    #endregion

    #region ERASE

    public static void EraseLine()
    {
        IsClearingBrush = true;
        ASLObject _aslBrush;
        foreach(GameObject brush in smallBrush)
        {
            _aslBrush = brush.GetComponent<ASLObject>();
            if (_aslBrush != null) _aslBrush.SendAndSetClaim(() =>
             {
                 _aslBrush.DeleteObject();
             });
        }
        foreach(GameObject brush in largeBrush)
        {
            _aslBrush = brush.GetComponent<ASLObject>();
            if (_aslBrush != null) _aslBrush.SendAndSetClaim(() =>
            {
                _aslBrush.DeleteObject();
            });
        }
        smallBrush.Clear();
        largeBrush.Clear();
        IsClearingBrush = false;
    }

    public static void EraseCount(int count)
    {
        IsClearingBrush = true;
        ASLObject _aslLarge;
        ASLObject _aslSmall;
        int actNdx;
        for(int i = 0; i < count; i++)
        {
            actNdx = brushCount - 1;
            _aslSmall = smallBrush[actNdx].GetComponent<ASLObject>();
            _aslLarge = largeBrush[actNdx].GetComponent<ASLObject>();
            if (_aslSmall != null) _aslSmall.SendAndSetClaim(() =>
            {
                _aslSmall.DeleteObject();
                smallBrush.RemoveAt(actNdx);
            });
            if (_aslLarge != null) _aslLarge.SendAndSetClaim(() =>
            {
                _aslLarge.DeleteObject();
                largeBrush.RemoveAt(actNdx);
            });
            brushCount--;
        }
        IsClearingBrush = false;
    }

    #endregion

    #region FOLLOW

    #endregion
}

public class BrushStorage : MonoBehaviour {
    private static BrushStorage _Instance;

    public static BrushStorage Instance
    {
        get
        {
            if (_Instance == null) _Instance = new GameObject("BrushStorage").AddComponent<BrushStorage>();
            return _Instance;
        }
    }
}

