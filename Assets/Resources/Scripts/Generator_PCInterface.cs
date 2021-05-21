using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Generator_PCInterface : MonoBehaviour
{
    public static Generator_PCInterface _PCgen;
    private static int fingerID = -1;

    public Dropdown MyDropdownList;
    public Transform smallMap, largeMap;

    void Awake()
    {
        if (_PCgen == null) _PCgen = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SelectByLeftClick(bool LeftShift, Ray mouseRay)
    {
        RaycastHit Hit;
        string DropdownOptionvalue = _PCgen.MyDropdownList.options[_PCgen.MyDropdownList.value].text;
        if(Physics.Raycast(mouseRay, out Hit, 1000f))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID)) return;
            //deselect
            if (LeftShift)
            {
                Vector3 spawnPos = Vector3.zero;
                if(Hit.collider.transform.parent.tag == "SpawnSmallMap")
                {
                    //spawnPos(Hit.point - _PCgen.smallMap.position) * MarkerDisplay.GetScaleFactor() + _PCgen.largeMap.position;
                    //Instantiate on small map(DropdownOptionValue, spawnPos, true)
                } else
                {
                    spawnPos = Hit.point;
                    //Instantiate on large map (DropdownOptionValue, spawnPos, false)
                }
            }
            else if (Hit.collider.gameObject.layer == LayerMask.NameToLayer("Markers"))
            {
                //set draw origin
            }
        } else
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID)) return;
            //deselect
        }        
    }

    public static void HoldLeftClick(Ray mouseRay, LayerMask mask)
    {

    }

    public static void LeftClickRelease(Ray mouseRay, LayerMask mask)
    {

    }
}
