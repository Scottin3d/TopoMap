using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDemoPlayer : MonoBehaviour {
    static bool RunUpdate;
    public GameObject playerPrefab = null;
    static GameObject PlayerObject = null;
    static GameObject ASLplayer;

    // Start is called before the first frame update
    void Start() {
        RunUpdate = false;
        PlayerObject = GameObject.Find("Player");
        Transform spawnPosition = PlayerSpawnPosition.current.GetSpawnPosition();

        PlayerObject.transform.position = spawnPosition.position;

        ASLHelper.InstantiateASLObject("PlayerPrefab", spawnPosition.position, Quaternion.identity, "", "", PlayerCreated);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (RunUpdate) {
            SendAndSetClaimPlayer();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Cylinder,
                        PlayerObject.transform.position,
                        Quaternion.identity);
        }
    }

    private void SendAndSetClaimPlayer() {
        ASLplayer.transform.position = PlayerObject.transform.position;
        ASLplayer.transform.rotation = PlayerObject.transform.rotation;

        ASLplayer.GetComponent<ASL.ASLObject>().SendAndSetClaim(() => {
            ASLplayer.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(PlayerObject.transform.rotation);
            ASLplayer.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(PlayerObject.transform.position);
        });

        ASLObjectTrackingSystem.UpdatePlayerTransform(ASLplayer.GetComponent<ASL.ASLObject>(), ASLplayer.transform);
    }



    public static void PlayerCreated(GameObject _gameObject) {
        ASLplayer = _gameObject;
        ASLplayer.GetComponent<MeshRenderer>().enabled = false;
        RunUpdate = true;
        ASLObjectTrackingSystem.AddPlayerToTrack(_gameObject.GetComponent<ASL.ASLObject>(), _gameObject.transform);
    }
}
