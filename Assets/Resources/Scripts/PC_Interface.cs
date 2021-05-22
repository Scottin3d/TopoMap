using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ASL;

public class PC_Interface : MonoBehaviour
{
    public static PC_Interface _pcInterface;
    private static int fingerID = -1;

    public Dropdown MyDropdownList;

    public Camera PlayerCamera;
    public Camera TableViewCamera;
    private static bool IsViewingTable = false;

    public GameObject LargeMap;
    public GameObject SmallMap;

    private static float drawTime = 1f;
    private static bool Projecting = false;
    private static bool IsPainting = false;
    private static bool IsPaused = false;

    public static bool IsProjecting { get { return Projecting; } }

    void Awake()
    {
        if (_pcInterface == null) _pcInterface = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(PlayerCamera != null, "Please set player camera in inspector.");
        Debug.Assert(TableViewCamera != null, "Please set table view camera in inspector.");

        PlayerCamera.enabled = true;
        TableViewCamera.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(PaintMap());
    }

    #region MARKER_PROJECTION

    public static void ProjectMarker(GameObject _marker)
    {
        if (_marker == null) return;
        if (IsViewingTable)
        {
            Ray mouseRay = _pcInterface.TableViewCamera.ScreenPointToRay(Input.mousePosition);
            ProjectCast(_marker, mouseRay);
        } else
        {
            Ray mouseRay = _pcInterface.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            ProjectCast(_marker, mouseRay);
        }
    }

    //TODO: move to separate script
    private static void ProjectCast(GameObject _marker, Ray mouseRay)
    {
        RaycastHit hit;
        if(Physics.Raycast(mouseRay, out hit, 100f))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID))
            {
                _marker.SetActive(false);
            } else
            {
                _marker.transform.position = hit.point;
                _marker.SetActive(hit.collider.gameObject.layer == LayerMask.NameToLayer("Holomap"));
            }
        }
    }

    #endregion

    #region DRAG_DRAW_CAST

    public static void OnClickLMB(bool LShift)
    {
        string optionValue = _pcInterface.MyDropdownList.options[_pcInterface.MyDropdownList.value].text;
        if (IsViewingTable)
        {
            Ray mouseRay = _pcInterface.TableViewCamera.ScreenPointToRay(Input.mousePosition);
            Marker_DragDrawV2.ClickCast(LShift, IsViewingTable, mouseRay, _pcInterface.SmallMap, _pcInterface.LargeMap, optionValue);
        } else
        {
            Ray mouseRay = _pcInterface.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            Marker_DragDrawV2.ClickCast(LShift, IsViewingTable, mouseRay, _pcInterface.SmallMap, _pcInterface.LargeMap, optionValue);
        }
    }   

    public static void OnHoldLMB()
    {
        drawTime -= Time.deltaTime;
        Projecting = (drawTime <= 0f);
        if (IsViewingTable)
        {
            Ray mouseRay = _pcInterface.TableViewCamera.ScreenPointToRay(Input.mousePosition);
            Marker_DragDrawV2.HoldCast(mouseRay, drawTime);
        } else
        {
            Ray mouseRay = _pcInterface.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            Marker_DragDrawV2.HoldCast(mouseRay, drawTime);
        }
    }    

    public static void OnReleaseLMB()
    {
        string optionValue = _pcInterface.MyDropdownList.options[_pcInterface.MyDropdownList.value].text;
        if (IsViewingTable)
        {
            Ray mouseRay = _pcInterface.TableViewCamera.ScreenPointToRay(Input.mousePosition);
            Marker_DragDrawV2.ReleaseCast(mouseRay, _pcInterface.SmallMap, _pcInterface.LargeMap, optionValue, drawTime);
        }
        else
        {
            Ray mouseRay = _pcInterface.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            Marker_DragDrawV2.ReleaseCast(mouseRay, _pcInterface.SmallMap, _pcInterface.LargeMap, optionValue, drawTime);
        }
        drawTime = 1f;
        Projecting = false;
    }    

    #endregion

    //TODO: Move to different script?
    #region BRUSH_DRAW

    public static void Paint(bool ShouldPaint)
    {
        IsPainting = ShouldPaint;
    }

    static IEnumerator PaintMap()
    {
        while (true)
        {
            if (IsPainting)
            {
                if (IsViewingTable)
                {

                } else
                {

                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    #endregion

    #region VIEW_FUNCTIONS

    public static void ToggleLocked()
    {
        if (IsPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        IsPaused = !IsPaused;
        //lock camera based on new state of paused
    }

    public static void ToggleCameras()
    {
        if (IsViewingTable)
        {
            _pcInterface.PlayerCamera.enabled = true;
            _pcInterface.TableViewCamera.enabled = false;
            IsViewingTable = false;
        } else
        {
            Ray mouseRay = _pcInterface.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(mouseRay,out hit, 100f))
            {
                if (EventSystem.current.IsPointerOverGameObject(fingerID)) return;

                if (hit.collider.tag == "Table" || hit.collider.gameObject.layer == LayerMask.NameToLayer("Holomap"))
                {
                    _pcInterface.PlayerCamera.enabled = false;
                    _pcInterface.TableViewCamera.enabled = true;
                    IsViewingTable = true;
                }
            }
        }
    }

    //TODO: Move to different script
    public static void UpdateFlashlight(GameObject playerBody, ASLObject _aslFlashlight)
    {

    }

    //TODO: move to different script
    public static void ToggleFlashlight(GameObject flashlight)
    {

    }

    #endregion
}
