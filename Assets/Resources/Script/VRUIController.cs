using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRUIController : MonoBehaviour
{

    //these are enumerators that represent the state of the VR UI. This is supposed to essentially represent what menu the VR Player is in, so that the class knows what to represent to the player.
    //Visual Representation of the UI Tree (so far):
    //
    //Main->
    //      Options->
    //                Movement Speed->
    //                                increase
    //                                changing speed
    //                                decrease
    //      Teleport->
    //                Ground Map
    //                Table Map
    //      Movement->
    //                Enable/Disable Upward Movement
    private enum VRUI_State
    {
        Off,               //off means that the UI should currently not be shown to the player.
        Main,              //main is the main menu of the UI, where the player can go into more specific menus.
        Options,           //options is the general options menu for the player
        Teleport,          //teleport is the menu for teleporting between the two main areas of our scene (table map and big map)
        Movement,          //this is the player movement menu
        Movement_Speed_Adjustment //this is the menu where the player is adjusting their speed
    }
    //array of all current VR UI Objects
    private GameObject[] currentUIObjects;

    //references to the VR Player's hands
    private GameObject leftHand;
    private GameObject rightHand;
    VRUI_State currentUIState;

    private void updateUIDisplay()
    {
        if (!VRStartupController.isInVR)
        {
            Debug.LogWarning("Warning: VR UI update called while VR is off");
        }
        removeCurrentUI();
        switch (currentUIState)
        {
            case VRUI_State.Main:
                setupMainMenu();
                break;
            case VRUI_State.Options:
                setupOptionsMenu();
                break;
            case VRUI_State.Teleport:
                setupTeleportMenu();
                break;
            case VRUI_State.Movement:
                setupMovementMenu();
                break;
            case VRUI_State.Movement_Speed_Adjustment:
                setupMoveSpeedAdjust();
                break;
            case VRUI_State.Off:
                break;
            default:
                Debug.LogWarning("Warning: VR UI update called during unknown VR UI state");
                break;
        }
    }

    //these functions (removeX and setupX) are UI Setup/Removal functions designed to provide the correct UI we want to update to
    //currently this changes the text on the buttons to reflect their current action
    private void removeCurrentUI()
    {
        //currently unneeded
    }

    private void setupMainMenu()
    {
        currentUIObjects[0].GetComponentInChildren<Text>().text = "Movement Options";
        currentUIObjects[1].GetComponentInChildren<Text>().text = "Teleport Locations";
        currentUIObjects[2].GetComponentInChildren<Text>().text = "Options";
        currentUIObjects[3].GetComponentInChildren<Text>().text = "";
    }

    private void setupOptionsMenu()
    {
        currentUIObjects[0].GetComponentInChildren<Text>().text = "";
        currentUIObjects[1].GetComponentInChildren<Text>().text = "";
        currentUIObjects[2].GetComponentInChildren<Text>().text = "Movement Speed";
        currentUIObjects[3].GetComponentInChildren<Text>().text = "Back";
    }

    private void setupTeleportMenu()
    {
        currentUIObjects[0].GetComponentInChildren<Text>().text = "";
        currentUIObjects[1].GetComponentInChildren<Text>().text = "Table Map";
        currentUIObjects[2].GetComponentInChildren<Text>().text = "Ground Map";
        currentUIObjects[3].GetComponentInChildren<Text>().text = "Back";
    }

    private void setupMovementMenu()
    {
        currentUIObjects[0].GetComponentInChildren<Text>().text = "Toggle Collision";
        currentUIObjects[1].GetComponentInChildren<Text>().text = "Toggle Gravity";
        currentUIObjects[2].GetComponentInChildren<Text>().text = "Toggle Upwards Movement";
        currentUIObjects[3].GetComponentInChildren<Text>().text = "Back";
    }

    private void setupMoveSpeedAdjust()
    {
        currentUIObjects[0].GetComponentInChildren<Text>().text = "Decrease";
        currentUIObjects[1].GetComponentInChildren<Text>().text = "Movement Speed";
        currentUIObjects[2].GetComponentInChildren<Text>().text = "Increase";
        currentUIObjects[3].GetComponentInChildren<Text>().text = "Back";
    }

    //this function will spawn in a button into the given position, rotation, and scale. The new object will be in the
    //currentUIObjects array position of buttonPosition, and the text displayed will be buttonText.
    //note that while position is a mandatory Vector3, scale and rotation can be set to zero
    //if the original button scale and rotation is preferred.
    private void spawnButton(Vector3 position, Vector3 rotation, Vector3 scale, string buttonText, int buttonPosition)
    {
        if(currentUIObjects[buttonPosition] != null)
        {
            Destroy(currentUIObjects[buttonPosition]);
            currentUIObjects[buttonPosition] = null;
        }
        
        currentUIObjects[buttonPosition] = (GameObject) Instantiate(Resources.Load("VR/VRUIButton"));
        currentUIObjects[buttonPosition].transform.SetParent(leftHand.transform);
        currentUIObjects[buttonPosition].transform.localPosition = position;
        if (rotation != Vector3.zero)
        {
            currentUIObjects[buttonPosition].transform.localRotation = Quaternion.Euler(rotation);
        }
        else
        {
            currentUIObjects[buttonPosition].transform.localRotation = Quaternion.Euler(new Vector3(17.044f, 9.419f, 69.47f));
        }
        if(scale != Vector3.zero)
        {
            currentUIObjects[buttonPosition].transform.localScale = scale;
        }
        currentUIObjects[buttonPosition].GetComponentInChildren<Text>().text = buttonText;
        //currentUIObjects[buttonPosition].GetComponent<HandUIActivateable>().UICollider = rightHand.transform.Find("UIInteractSphere").gameObject;
        //currentUIObjects[buttonPosition].GetComponent<HandUIActivateable>().reciever = this;
    }


    //this function is intended to be called by the VR Startup controller when the VR player object is ready,
    //so that this class has access to the VR Player's left hand (for creating the menu in the right spot)
    public void initialize(GameObject VRPlayer)
    {
        leftHand = VRPlayer.transform.Find("SteamVRObjects/LeftHand").gameObject;
        rightHand = VRPlayer.transform.Find("SteamVRObjects/RightHand").gameObject;

        spawnButton(new Vector3(-0.0808f, 0.0558f, -0.1089f), Vector3.zero, Vector3.zero, "firstButton", 0);
        spawnButton(new Vector3(-0.1194f, 0.0701f, -0.0974f), Vector3.zero, Vector3.zero, "secondButton", 1);
        spawnButton(new Vector3(-0.1572f, 0.0842f, -0.0875f), Vector3.zero, Vector3.zero, "thirdButton", 2);
        spawnButton(new Vector3(-0.1280f, 0.0949f, -0.1676f), new Vector3(-19.516f, -74.022f, 71.847f), new Vector3(0.001f, 0.03f, 0.11006f), "backButton", 3);

        currentUIState = VRUI_State.Main;
        updateUIDisplay();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //this function is intended to be called by the buttons when pressed to allow for the correct behavior to be performed
    public void activatedUIButton(GameObject button)
    {
        if (!VRStartupController.isInVR)
        {
            Debug.LogWarning("Warning: VR UI Button activated while VR is off");
        }
        switch (currentUIState)
        {
            case VRUI_State.Main:
                handleMainMenu(button);
                break;
            case VRUI_State.Options:
                handleOptionsMenu(button);
                break;
            case VRUI_State.Teleport:
                handleTeleportMenu(button);
                break;
            case VRUI_State.Movement:
                handleMovementMenu(button);
                break;
            case VRUI_State.Off:
                Debug.LogWarning("Warning: VR UI Button activated during Off UI state");
                break;
            default:
                Debug.LogWarning("Warning: VR UI Button activated during unknown UI state");
                break;
        }
    }

    //these handle functions are intended to handle the behavior of a button press based on the current UI state
    private void handleMainMenu(GameObject button)
    {
        if(button == null)
        {
            Debug.LogWarning("null button reference in handleMainMenu");
        }
        else if(button == currentUIObjects[0])
        {
            currentUIState = VRUI_State.Movement;
            updateUIDisplay();
            return;
        }
        else if(button == currentUIObjects[1])
        {
            currentUIState = VRUI_State.Teleport;
            updateUIDisplay();
            return;
        }
        else if (button == currentUIObjects[2])
        {
            currentUIState = VRUI_State.Options;
            updateUIDisplay();
            return;
        }
    }

    private void handleOptionsMenu(GameObject button)
    {

    }

    private void handleTeleportMenu(GameObject button)
    {

    }

    private void handleMovementMenu(GameObject button)
    {

    }


    //this is a coroutine meant to start up the VR UI when the VR Player is detected
    IEnumerator waitForVRPlayer()
    {
        while(VRStartupController.VRPlayerObject == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        initialize(VRStartupController.VRPlayerObject);
    }
}
