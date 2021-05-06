using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDemoPlayer : MonoBehaviour {

    public GameObject playerPrefab = null;

    static GameObject _localPlayer = null;
    static ASLObject _ASLplayer = null;

    private static readonly float UPDATES_PER_SECOND = 5.0f;

    void Start() {
        _localPlayer = GameObject.Find("Player");

        ASLHelper.InstantiateASLObject(playerPrefab.name, Vector3.zero, Quaternion.identity, null, null, OnPlayerCreated);

        StartCoroutine(NetworkedUpdate());
    }

    private void Update() {
        //miniCam.transform.position = _localPlayerObject.transform.position + 15f * Vector3.up;
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            ASLHelper.InstantiateASLObject(PrimitiveType.Capsule, _localPlayer.transform.position, Quaternion.identity, null, null, OnPrimitiveCreate);
        }
    }

    IEnumerator NetworkedUpdate() {
        while (true) {
            while (_ASLplayer == null) {
                yield return new WaitForSeconds(0.1f);
            }

            _ASLplayer.transform.position = _localPlayer.transform.position;
            _ASLplayer.transform.rotation = _localPlayer.transform.rotation;

            _ASLplayer.SendAndSetClaim(() => {
                _ASLplayer.SendAndSetWorldPosition(_localPlayer.transform.position);
                _ASLplayer.SendAndSetWorldRotation(_localPlayer.transform.rotation);
            });
            
            //ASLObjectTrackingSystem.UpdatePlayerTransform(_ASLplayer, _ASLplayer.transform);


            yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);
        }
    }

    private static void OnPlayerCreated(GameObject obj) {
        _ASLplayer = obj.GetComponent<ASLObject>();
        _ASLplayer.gameObject.GetComponent<Renderer>().enabled = false;
        ASLObjectTrackingSystem.AddPlayerToTrack(_ASLplayer);
    }

    private static void OnPrimitiveCreate(GameObject _gameObject) {
        _gameObject.tag = "Object";
        _gameObject.GetComponent<ASLObject>().SendAndSetClaim(() =>
        {
            _gameObject.GetComponent<ASLObject>().SendAndSetWorldPosition(_gameObject.transform.position);
        });
       ASLObjectTrackingSystem.AddObjectToTrack(_gameObject.GetComponent<ASLObject>());
    }
}
