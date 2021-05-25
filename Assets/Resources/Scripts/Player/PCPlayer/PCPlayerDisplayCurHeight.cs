using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PCPlayerDisplayCurHeight : MonoBehaviour
{
    public GameObject MyPlayerTeleport;
    public Text MyHeightInformation;
    public GameObject MyCurHeightObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMyHeightInfo();
    }

    private void UpdateMyHeightInfo()
    {
        if (!MyPlayerTeleport.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            MyHeightInformation.enabled = true;
            float H = MyCurHeightObject.GetComponent<PlayerHeight>().Get_PC_CurHeight();
            MyHeightInformation.GetComponent<Text>().text = "Current Altitude \r\n" + H + "m";
        }
        else
        {
            MyHeightInformation.enabled = false;
        }
    }
}
