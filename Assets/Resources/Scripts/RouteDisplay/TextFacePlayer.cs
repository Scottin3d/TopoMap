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
        } else
        {
            //face world origin
            myText.gameObject.transform.LookAt(Vector3.zero);
        }
        myText.gameObject.transform.forward = -myText.gameObject.transform.forward;
        
        Vector3 truePos = textParent.transform.position - 2f * textParent.transform.up;
        string posText = string.Format("({0:f4},{1:f4},{2:f4})", truePos.x, truePos.y, truePos.z);
        string spdText = string.Format("\n{0:f4} (m/s?)", PathDisplay.GetWalkerVelocity(textParent.transform.root));
        myText.text = string.Concat(posText, spdText);
    }

    public void SetPlayer(GameObject _p)
    {
        player = _p;
    }
}
