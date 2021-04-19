using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDemoPlayer : MonoBehaviour {

    public GameObject playerPrefab = null;
    public static GameObject player;
    private static ASLObject ASLplayer;
    // Start is called before the first frame update
    void Start() {
        ASLHelper.InstantiateASLObject("PlayerPrefab", Vector3.zero, Quaternion.identity, "", "UnityEngine.Rigidbody,UnityEngine", PlayerCreated);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (ASLplayer) {
            ASLplayer.SendAndSetClaim(() => {
                ASLplayer.SendAndSetWorldPosition(player.transform.position);
            });
            
        }
    }

    public static void PlayerCreated(GameObject _gameObject) {
        player = _gameObject;
        player.name = "Player";
        GameObject.Find("Camera").transform.parent = player.transform;
        ASLplayer = player.GetComponent<ASLObject>();
        ASLplayer.SendAndSetClaim(() => {
            ASLplayer.SendAndSetWorldPosition(player.transform.position);
        });
        EndlessTerrain.current.StartTerrain(player.transform);
    }
}
