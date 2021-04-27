using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    private static bool RunUpdate;
    private static GameObject PlayerObject;
    private static GameObject Cube;
    private static GameObject Marker;
    private float UpdateTimer = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        RunUpdate = false;
        PlayerObject = GameObject.Find("Player");
        ASL.ASLHelper.InstantiateASLObject("MinimapMarker_Player", new Vector3(0, 0, 0), Quaternion.identity, "", "", PlayerMinimapInstantiation);
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

    private static void PlayerMinimapInstantiation(GameObject _myGameObject)
    {
        Marker = _myGameObject;
        Color theColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1f);
        Marker.GetComponent<Renderer>().material.color = theColor;

        Marker.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            Marker.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(theColor, theColor);
        });
        //Marker.GetComponent<MeshRenderer>().enabled = false;
        ASLObjectTrackingSystem.AddObjectToTrack(Marker.GetComponent<ASL.ASLObject>(), Marker.transform);
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
        Vector3 pos = PlayerObject.transform.position;
        pos.y += 10f;
        Marker.transform.position = pos + 0.5f * Vector3.down;

        Marker.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            Marker.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(pos + 0.5f * Vector3.down);
            ASLObjectTrackingSystem.UpdateObjectTransform(Marker.GetComponent<ASL.ASLObject>(), Marker.transform);
        });
    }
}
