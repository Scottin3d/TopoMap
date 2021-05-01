using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextFacePlayer : MonoBehaviour
{
    public TMP_Text myText;
    private GameObject textParent;
    private GameObject player;

    void Awake()
    {
        textParent = gameObject;
        //attempt to find player
        player = GameObject.Find("PCHandler/Player");
    }


    void Update()
    {
        if(player != null)
        {
            myText.gameObject.transform.LookAt(player.transform.position);
            myText.gameObject.transform.forward = -myText.gameObject.transform.forward;
        } else
        {
            //face world origin
            myText.gameObject.transform.LookAt(Vector3.zero);
        }
        //string displayText = "";
        Vector3 truePos = textParent.transform.position - 2f * textParent.transform.up;
        //displayText = string.Concat("(",truePos.x, ",", truePos.y, ",", truePos.z, ")");
        //displayText = string.Format("({0:C4},{1:C4},{2:C4})", truePos.x, truePos.y, truePos.z);
        myText.text = string.Format("({0:f4},{1:f4},{2:f4})", truePos.x, truePos.y, truePos.z);
    }

    public void SetPlayer(GameObject _p)
    {
        player = _p;
    }
}
