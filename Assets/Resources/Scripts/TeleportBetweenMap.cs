using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBetweenMap : MonoBehaviour
{
    public GameObject LargeMap;
    public GameObject SmallMap;
    public GameObject Player;
    private bool AtSmallMap = true;

    private int ClickTime = 0;
    private bool IfDoubleClick = false;

    private Vector3 LastPositionInSmallMap;
    private Vector3 LastPositionInLargeMap;

    //public GameObject SpaceShip;
    //private Animator SpaceShipAnimator;
    // Start is called before the first frame update
    void Start()
    {
        LastPositionInLargeMap = LargeMap.transform.position + new Vector3(0, 50, 0);
        LastPositionInSmallMap = SmallMap.transform.position + new Vector3(0, 0, 3);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMyLastPosition();
        Teleport();
    }

    private void Teleport()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ClickTime++;
            if (ClickTime != 2)
            {
                StartCoroutine(CheckSecondClick());
            }
            else
            {
                ClickTime = 0;
                TeleportDirectly();
            }
        }
    }

    IEnumerator CheckSecondClick()
    {
        yield return new WaitForSeconds(0.2f);
        if (ClickTime > 0)
        {
            ClickTime--;
        }
    }

    private void TeleportDirectly()
    {
        if (AtSmallMap)
        {
            AtSmallMap = false;
            Player.GetComponent<CharacterController>().enabled = false;
            Player.transform.position = LastPositionInLargeMap;
            Player.GetComponent<CharacterController>().enabled = true;
            //Player.transform.position = LargeMap.transform.parent.transform.position + new Vector3(0, 10, 0);
        }
        else
        {
            Debug.Log("To small");
            AtSmallMap = true;
            Player.GetComponent<CharacterController>().enabled = false;
            Player.transform.position = LastPositionInSmallMap;
            Player.GetComponent<CharacterController>().enabled = true;
            //Player.transform.position = SmallMap.transform.parent.transform.position;
        }
    }

    private void UpdateMyLastPosition()
    {
        if (AtSmallMap)
        {
            LastPositionInSmallMap = Player.transform.position;
        }
        else
        {
            LastPositionInLargeMap = Player.transform.position;
        }
    }

    public bool GetAtSmallMap()
    {
        return AtSmallMap;
    }
}
