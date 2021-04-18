using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDemoPlayer : MonoBehaviour {
    static bool RunUpdate;
    public GameObject playerPrefab = null;
    public static GameObject player;
    public static GameObject localPlayer;

    private static ASLObject ASLplayer;
    // Start is called before the first frame update
    void Start() {
        RunUpdate = false;
        localPlayer = GameObject.Find("Player");
        Transform spawnPosition = PlayerSpawnPosition.current.GetSpawnPosition();
        localPlayer.transform.position = spawnPosition.position;
        ASLHelper.InstantiateASLObject("PlayerPrefab", spawnPosition.position, Quaternion.identity, null, null, PlayerCreated);
    }

    // Update is called once per frame
    void FixedUpdate() {

        if (RunUpdate) {
            SendAndSetClaimPlayer();
        }
    }

    private void SendAndSetClaimPlayer() {
        player.transform.position = localPlayer.transform.position;
        player.transform.rotation = localPlayer.transform.rotation;

        ASLplayer.SendAndSetClaim(() => {
            ASLplayer.SendAndSetWorldRotation(player.transform.rotation);
            ASLplayer.SendAndSetWorldPosition(player.transform.position);
        });

        ASLObjectTrackingSystem.UpdatePlayerTransform(ASLplayer, player.transform);
    }



    public static void PlayerCreated(GameObject _gameObject) {
        player = _gameObject;
        ASLplayer = _gameObject.GetComponent<ASLObject>();
        //GameObject camera = GameObject.Find("Camera");
        //camera.transform.parent = player.transform;
        //camera.transform.localPosition = new Vector3(0f, 1f, -2f);
        //Quaternion rotation = Quaternion.identity;
        //rotation.eulerAngles = new Vector3(20f, 0f, 0f);
        //camera.transform.localRotation = rotation;
        // set camera transform
        //player.GetComponent<MeshRenderer>().enabled = false;
        RunUpdate = true;
        ASLObjectTrackingSystem.AddPlayerToTrack(ASLplayer, player.transform);
    }
}
