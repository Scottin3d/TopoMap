using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDemoPlayer : MonoBehaviour {

    //public GameObject localPlayerPrefab = null;
    public GameObject playerPrefab = null;
    public VRStartupController VRController = null; //VR controller to give 2D player object to.

    static GameObject _localPlayerObject = null;
    static GameObject _playerObject = null;
    static ASLObject _playerAslObject = null;

    private static readonly float UPDATES_PER_SECOND = 2.0f;

    void Start() {

        //_localPlayerObject = (GameObject)Instantiate(Resources.Load(localPlayerPrefab.name));
        _localPlayerObject = (GameObject)Instantiate(Resources.Load("MyPrefabs/Player"));
        ASLHelper.InstantiateASLObject(playerPrefab.name, Vector3.zero, Quaternion.identity, null, null, OnPlayerCreated);

        VRController.setPlayer2D(_localPlayerObject);

        StartCoroutine(DelayedInit());
        StartCoroutine(NetworkedUpdate());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            ASLHelper.InstantiateASLObject(PrimitiveType.Capsule, _localPlayerObject.transform.position, Quaternion.identity, null, null, OnPrimitiveCreate);
        
        }
    }

    IEnumerator DelayedInit() {
        while (_playerObject == null) {
            yield return new WaitForSeconds(0.1f);
        }

        Transform spawnPosition = PlayerSpawnPosition.current.GetSpawnPosition();
        _localPlayerObject.transform.position = spawnPosition.position;
        //GameObject.Find("CameraMain").transform.parent = _playerObject.transform;
        
        _playerAslObject.SendAndSetClaim(() => {
            _playerAslObject.SendAndSetWorldPosition(_localPlayerObject.transform.position);
            _playerAslObject.SendAndSetWorldRotation(_localPlayerObject.transform.rotation);
        });

        ASLObjectTrackingSystem.AddPlayerToTrack(_playerAslObject, _localPlayerObject.transform);
        //_playerObject.SetActive(false);

        
    }

    IEnumerator NetworkedUpdate() {
        while (true) {
            while (_playerObject == null) {
                yield return new WaitForSeconds(0.1f);
            }

            _playerAslObject.SendAndSetClaim(() => {
                _playerAslObject.SendAndSetWorldPosition(_localPlayerObject.transform.position);
                _playerAslObject.SendAndSetWorldRotation(_localPlayerObject.transform.rotation);
            });

            ASLObjectTrackingSystem.UpdatePlayerTransform(_playerAslObject, _playerAslObject.transform);

            yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);
        }
    }

    private static void OnPlayerCreated(GameObject obj) {
        _playerObject = obj;
        _playerAslObject = obj.GetComponent<ASLObject>();
        
    }

    private static void OnPrimitiveCreate(GameObject _gameObject) {
       ASLObjectTrackingSystem.AddObjectToTrack(_gameObject.GetComponent<ASLObject>(), _gameObject.transform);
    }
}
