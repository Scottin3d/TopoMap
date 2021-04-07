using UnityEngine;
using UnityEngine.EventSystems;

namespace StressTesting
{
    /// <summary>The selection, color handling, and movement controls for objects in this scene</summary>
    public class StressTest_150_ASLObjects_Controller : MonoBehaviour
    {
        private GameObject m_SelectedObject = null;
        private Camera m_MainCamera;
        // Start is called before the first frame update
        void Start()
        {
            m_MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            //Mouse control
            LMBSelect();
        }

        // Mouse click selection 
        void LMBSelect()
        {
            if (Input.GetMouseButtonDown(0)) //Left mouse click
            {
                // Copied from: https://forum.unity.com/threads/preventing-ugui-mouse-click-from-passing-through-gui-controls.272114/
                if (!EventSystem.current.IsPointerOverGameObject()) // check for GUI
                {

                    RaycastHit hitInfo = new RaycastHit();

                    bool hit = Physics.Raycast(m_MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, 1);
                    // 1 is the mask for default layer

                    if (hit)
                        SelectObject(hitInfo.transform.gameObject);
                    else
                        SelectObject(null);
                }
            }
        }



        private void SelectObject(GameObject _selectedObject)
        {
            if (_selectedObject?.GetComponent<ASL.ASLObject>() == null) { _selectedObject = null; } //Set to null if not an ASL object
            SelectObjectColor(_selectedObject);
        }

        private void SelectObjectColor(GameObject _selectedObject)
        {        
            //if our last selected object is an ASL object and is not null then...
            if (m_SelectedObject?.GetComponent<ASL.ASLObject>() != null)
            {
                GameObject previouslySelectedObject = m_SelectedObject; //As m_SelectedObject will be updated before claim is processed, get a handle to it
                m_SelectedObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() => //Claim our previously selected object briefly in order to reset color
                {
                    //Though called with m_Selected, use previouslySelectedObject now as mSelected may equal something else, since this code is a callback function
                    previouslySelectedObject.GetComponent<ASL.ASLObject>().m_ReleaseFunction.Invoke(previouslySelectedObject);
                    previouslySelectedObject.GetComponent<ASL.ASLObject>()._LocallyRemoveReleaseCallback();
                }, 1000, false); //Don't reset release timer - release asap.
            }

            m_SelectedObject = _selectedObject; //Updated selected object 
            if (m_SelectedObject?.GetComponent<ASL.ASLObject>() != null)
            {
                GameObject currentlySelected = m_SelectedObject;
                //Update color to let us and others know this object is selected by a user
                m_SelectedObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
                {
                    currentlySelected.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(Color.yellow, Color.cyan);
                    currentlySelected.GetComponent<ASL.ASLObject>()._LocallySetReleaseFunction(OnRelease); //Call this function when we no longer own this object
                }, 0); //By setting timeout to 0 we are keeping this object until someone steals it from us - thus forcing the OnRelease function to only occur when stolen
            }
        }

        private void OnRelease(GameObject _myGameObject)
        {
            //Set back to original color - since we are calling in OnRelease we already own this object and thus don't need to claim it
            _myGameObject.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(
                    _myGameObject.GetComponent<StressTest_150_ASLObjects_Support>().m_MyObjectOriginalColor,
                    _myGameObject.GetComponent<StressTest_150_ASLObjects_Support>().m_MyObjectOriginalColor);

            //If we are releasing the current object we have selected - set this object to null 
            if (_myGameObject?.GetComponent<ASL.ASLObject>()?.m_Id == m_SelectedObject?.GetComponent<ASL.ASLObject>()?.m_Id)
            {
                m_SelectedObject = null;
            }
        }
    }
}