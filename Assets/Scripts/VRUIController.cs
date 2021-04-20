using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRUIController : MonoBehaviour
{

    //these are enumerators that represent the state of the VR UI. This is supposed to essentially represent what menu the VR Player is in, so that the class knows what to represent to the player.
    //Visual Representation of the UI Tree (so far):
    //
    //Main->
    //      Options->
    //                Movement Speed->
    //                                slider/knob
    //      Teleport->
    //                Ground Map
    //                Table Map
    //      Movement->
    //                Enable/Disable Upward Movement
    private enum VRUI_State
    {
        Off,               //off means that the UI should currently not be shown to the player.
        Main,              //main is the main menu of the UI, where the player can go into more specific menus.
        Options           //options is the general options menu for the player
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
