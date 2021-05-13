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

    //public GameObject SpaceShip;
    //private Animator SpaceShipAnimator;
    // Start is called before the first frame update
    void Start()
    {
        //SpaceShipAnimator = SpaceShip.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
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
            Player.transform.position = LargeMap.transform.position + new Vector3(0, 50, 0);
            Player.GetComponent<CharacterController>().enabled = true;
            //Player.transform.position = LargeMap.transform.parent.transform.position + new Vector3(0, 10, 0);
        }
        else
        {
            Debug.Log("To small");
            AtSmallMap = true;
            Player.GetComponent<CharacterController>().enabled = false;
            Player.transform.position = SmallMap.transform.position + new Vector3(0, 0, 3);
            Player.GetComponent<CharacterController>().enabled = true;
            //Player.transform.position = SmallMap.transform.parent.transform.position;
        }
    }

    private void TeleportToMapAnimation()
    {

    }
}
