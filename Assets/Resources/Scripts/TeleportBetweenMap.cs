using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBetweenMap : MonoBehaviour
{
    public GameObject LargeMap;
    public GameObject SmallMap;
    public GameObject Player;
    private bool AtSmallMap = true;
    // Start is called before the first frame update
    void Start()
    {
        
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
            if (AtSmallMap)
            {
                AtSmallMap = false;
                Player.GetComponent<CharacterController>().enabled = false;
                Player.transform.position = LargeMap.transform.position;
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
    }
}
