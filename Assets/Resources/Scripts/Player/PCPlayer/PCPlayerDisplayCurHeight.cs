using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PCPlayerDisplayCurHeight : MonoBehaviour
{
    public GameObject MyPlayerTeleport;
    public Text MyHeightInformation;
    public TextMeshProUGUI MyHeightInformationOutLine;
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
            MyHeightInformationOutLine.enabled = true;
            float H = MyCurHeightObject.GetComponent<PlayerHeight>().Get_PC_CurHeight();
            H = Mathf.Round(H * 100f) / 100f;
            //MyHeightInformation.GetComponent<Text>().text = "Current Altitude \r\n" + H + "m";
            MyHeightInformationOutLine.GetComponent<TMPro.TextMeshProUGUI>().text = "Current Altitude \r\n" + H + "m";
        }
        else
        {
            MyHeightInformation.enabled = false;
            MyHeightInformationOutLine.enabled = false;
        }
    }
}
