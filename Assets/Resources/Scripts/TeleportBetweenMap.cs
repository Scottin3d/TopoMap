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
                Player.transform.position = LargeMap.transform.position + new Vector3(0, 10, 0);
            }
            else
            {
                AtSmallMap = true;
                Player.transform.position = SmallMap.transform.position + new Vector3(0, 10, 0);
            }
        }
    }
}
