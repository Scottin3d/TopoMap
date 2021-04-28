using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTableView : MonoBehaviour
{
    public Camera PlayerCamera;
    public Camera PlayerTopViewCamera;
    public GameObject PlayerCameraObject;
    public GameObject PlayerTopViewCameraObject;
    private bool TableView = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ChangeViewMode();
    }

    private void ChangeViewMode()
    {
        if (Input.GetKeyDown(KeyCode.V) && TableView == false)
        {
            Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit Hit;
            if (Physics.Raycast(MouseRay, out Hit))
            {
                if (Hit.collider.tag == "Table")
                {
                    PlayerCamera.enabled = false;
                    PlayerTopViewCamera.enabled = true;
                    TableView = true;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.V) && TableView == true)
        {
            PlayerCamera.enabled = true;
            PlayerTopViewCamera.enabled = false;
            TableView = false;
        }
    }
}
