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

    public Dropdown markerDropdown;
    public Dropdown eraseDropdown;

    public Camera PlayerCamera;
    public Camera TableViewCamera;
    private static bool IsViewingTable = false;

    public GameObject LargeMap;
    public GameObject SmallMap;

    private static float drawTime = 1f;
    private static bool Projecting = false;
    private static bool IsPainting = false;
    private static bool IsPaused = false;
    private static bool IsFlashlightOn = false;
    private static bool IsVR = false;

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
        Debug.Assert(LargeMap != null, "Please set large map in inspector.");
        Debug.Assert(SmallMap != null, "Please set small map in inspector.");

        PlayerCamera.enabled = true;
        TableViewCamera.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(PaintMap());
    }

    #region MARKER_PROJECTION

    /// <summary>
    /// Projects marker onto the holomap (small map).
    /// </summary>
    /// <param name="_marker">The marker used for projection</param>
    public static void ProjectMarker(GameObject _marker)
    {
        if (_marker == null) return;
        if (IsViewingTable)
        {
            Ray mouseRay = _pcInterface.TableViewCamera.ScreenPointToRay(Input.mousePosition);
            ProjectCast(_marker, null, mouseRay);
        } else
        {
            Ray mouseRay = _pcInterface.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            ProjectCast(_marker, null, mouseRay);
        }
    }

    /// <summary>
    /// Projects marker onto the holomap (small map). VR only.
    /// </summary>
    /// <param name="_marker">The marker used for projection</param>
    /// <param name="VRmarker">The VR pointer</param>
    public static void ProjectMarker(GameObject _marker, GameObject VRmarker)
    {
        if (_marker == null) return;
        if (IsViewingTable)
        {
            Ray mouseRay = _pcInterface.TableViewCamera.ScreenPointToRay(Input.mousePosition);
            ProjectCast(_marker, VRmarker, mouseRay);
        }
        else
        {
            Ray mouseRay = _pcInterface.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            ProjectCast(_marker, VRmarker, mouseRay);
        }
    }

    //TODO: move to separate script
    private static void ProjectCast(GameObject _marker, GameObject VRmarker, Ray mouseRay)
    {
        RaycastHit hit;
        if(Physics.Raycast(mouseRay, out hit, 100f))
        {
            if (EventSystem.current.IsPointerOverGameObject(fingerID))
            {
                if (VRmarker != null) VRmarker.SetActive(false);
                _marker.SetActive(false);
            } else
            {
                if (VRmarker != null)
                {
                    VRmarker.transform.position = hit.point;
                    VRmarker.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    VRmarker.SetActive(hit.collider.gameObject.layer == LayerMask.NameToLayer("Holomap"));
                }
                _marker.transform.position = hit.point;
                _marker.SetActive(hit.collider.gameObject.layer == LayerMask.NameToLayer("Holomap"));
            }
        }
    }

    #endregion

    #region PC_DRAG/DRAW CAST

    public static void OnClickLMB(bool LShift)
    {
        string optionValue = _pcInterface.markerDropdown.options[_pcInterface.markerDropdown.value].text;
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
        string optionValue = _pcInterface.markerDropdown.options[_pcInterface.markerDropdown.value].text;
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

    #region BRUSH_DRAW

    public static void Paint(bool ShouldPaint)
    {
        IsPainting = ShouldPaint;
    }

    public static IEnumerator PaintMap()
    {
        while (true)
        {
            if (IsPainting)
            {
                if (MyController.InVR)
                {

                } else
                {
                    if (IsViewingTable)
                    {
                        Ray mouseRay = _pcInterface.TableViewCamera.ScreenPointToRay(Input.mousePosition);
                        BrushGeneratorV2.DrawBrush(mouseRay, _pcInterface.SmallMap.transform.position, _pcInterface.LargeMap.transform.position);
                    }
                    else
                    {
                        Ray mouseRay = _pcInterface.PlayerCamera.ScreenPointToRay(Input.mousePosition);
                        BrushGeneratorV2.DrawBrush(mouseRay, _pcInterface.SmallMap.transform.position, _pcInterface.LargeMap.transform.position);
                    }
                }
                
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    public static void ErasePC()
    {
        if (_pcInterface.eraseDropdown.options[_pcInterface.eraseDropdown.value].text == "EraseAll")
        {
            BrushGeneratorV2.EraseLine();
        }
        if (_pcInterface.eraseDropdown.options[_pcInterface.eraseDropdown.value].text == "EraseLastTen")
        {
            BrushGeneratorV2.EraseCount(10);
        }
    }

    #endregion

    #region VIEW_FUNCTIONS

    /// <summary>
    /// Toggles whether the mouse cursor is visible and unlocked
    /// </summary>
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

    public static void ToggleCamerasPC()
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

    public static void UpdateFlashlight(GameObject playerBody, ASLObject _aslFlashlight)
    {
        if (!_aslFlashlight.gameObject.activeSelf)
        {
            return;
        }
        _aslFlashlight.gameObject.transform.position = _pcInterface.PlayerCamera.transform.position;
        _aslFlashlight.gameObject.transform.rotation = _pcInterface.PlayerCamera.transform.rotation;

        _aslFlashlight.SendAndSetClaim(() =>
        {
            _aslFlashlight.SendAndSetWorldRotation(_pcInterface.PlayerCamera.transform.rotation);
            _aslFlashlight.SendAndSetWorldPosition(_pcInterface.PlayerCamera.transform.position);
        });
    }

    public static void ToggleFlashlight(GameObject flashlight)
    {
        IsFlashlightOn = !IsFlashlightOn;
        flashlight.SetActive(IsFlashlightOn);
    }

    #endregion
}
