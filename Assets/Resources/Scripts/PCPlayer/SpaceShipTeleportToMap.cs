using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipTeleportToMap : MonoBehaviour
{
    public GameObject PCPlayer;
    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        TeleportPlayer(other);
    }

    private void TeleportPlayer(Collider other)
    {
        if (other.tag.Equals("GameController"))
        {
            Debug.Log("s");
        }
    }
}
