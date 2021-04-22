using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    private static bool RunUpdate;
    private static GameObject PlayerObject;
    private static GameObject Cube;
    private float UpdateTimer = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        RunUpdate = false;
        PlayerObject = GameObject.Find("Player");
        ASL.ASLHelper.InstantiateASLObject("PlayerCube", new Vector3(0,0,0), Quaternion.identity, "", "", PlayerInstantiation);
    }

    void Update()
    {
        if (RunUpdate)
        {
            UpdateTimer -= Time.deltaTime;
            if (UpdateTimer <= 0)
            {
                SendAndSetClaimPlayer();
                UpdateTimer = 0.1f;
            }
        }
    }

    private static void PlayerInstantiation(GameObject _myGameObject)
    {
        Cube = _myGameObject;
        RunUpdate = true;
        Cube.GetComponent<MeshRenderer>().enabled = false;
        ASLObjectTrackingSystem.AddPlayerToTrack(Cube.GetComponent<ASL.ASLObject>(), Cube.transform);
    }

    private void SendAndSetClaimPlayer()
    {
        Cube.transform.position = PlayerObject.transform.position;
        Cube.transform.rotation = PlayerObject.transform.rotation;

        Cube.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            Cube.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(PlayerObject.transform.rotation);
            Cube.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(PlayerObject.transform.position);
            ASLObjectTrackingSystem.UpdatePlayerTransform(Cube.GetComponent<ASL.ASLObject>(), Cube.transform);
        });
    }
}
