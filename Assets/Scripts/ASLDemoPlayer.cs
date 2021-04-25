using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLDemoPlayer : MonoBehaviour {

    //public GameObject localPlayerPrefab = null;
    public GameObject playerPrefab = null;
    public VRStartupController VRController = null; //VR controller to give 2D player object to.
    public static Color _myColor;

    static GameObject _localPlayerObject = null;
    static GameObject _playerObject = null;
    static ASLObject _playerAslObject = null;

    static GameObject _localMinimapObject = null;
    static GameObject _minimapObject = null;
    static ASLObject _minimapAslObject = null;

    private GameObject miniCam;

    private static readonly float UPDATES_PER_SECOND = 2.0f;

    void Start() {
        _myColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), .25f);
        _localPlayerObject = (GameObject)Instantiate(Resources.Load("MyPrefabs/Player"));
        _localMinimapObject = (GameObject)Instantiate(Resources.Load("MyPrefabs/MinimapMarker_Player"));

        ASLHelper.InstantiateASLObject(playerPrefab.name, Vector3.zero, Quaternion.identity, null, null, OnPlayerCreated);
        ASLHelper.InstantiateASLObject("MinimapMarker_Player", new Vector3(0, 0, 0), Quaternion.identity, null, null, OnPlayerMarkerCreated);
        miniCam = Instantiate(Resources.Load("MyPrefabs/MinimapCamera")) as GameObject;

        VRController.setPlayer2D(_localPlayerObject);

        StartCoroutine(DelayedInit());
        StartCoroutine(NetworkedUpdate());
    }

    private void Update() {
        miniCam.transform.position = _localPlayerObject.transform.position + 15f * Vector3.up;
        _localMinimapObject.transform.position = _localPlayerObject.transform.position + 10f * Vector3.up;
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
        
        _playerAslObject.SendAndSetClaim(() => {
            _playerAslObject.SendAndSetWorldPosition(_localPlayerObject.transform.position);
            _playerAslObject.SendAndSetWorldRotation(_localPlayerObject.transform.rotation);
        });

        ASLObjectTrackingSystem.AddPlayerToTrack(_playerAslObject, _localPlayerObject.transform);

        while (_minimapObject == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        _localMinimapObject.transform.position = spawnPosition.position + 10f * Vector3.up;

        _minimapAslObject.SendAndSetClaim(() =>
        {
            _minimapAslObject.SendAndSetWorldPosition(_localMinimapObject.transform.position);
            _minimapAslObject.SendAndSetObjectColor(_myColor, _myColor);
        });
        //ASLObjectTrackingSystem.AddObjectToTrack(_minimapAslObject, _minimapObject.transform);
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

            while (_minimapObject == null)
            {
                yield return new WaitForSeconds(0.1f);
            }

            _minimapAslObject.SendAndSetClaim(() =>
            {
                _minimapAslObject.SendAndSetWorldPosition(_localMinimapObject.transform.position);
            });
            //ASLObjectTrackingSystem.UpdateObjectTransform(_minimapAslObject, _minimapAslObject.transform);

            yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);
        }
    }

    private static void OnPlayerCreated(GameObject obj) {
        _playerObject = obj;
        _playerAslObject = obj.GetComponent<ASLObject>();
    }

    private static void OnPlayerMarkerCreated(GameObject obj)
    {
        _minimapObject = obj;
        _minimapAslObject = obj.GetComponent<ASLObject>();
    }

    private static void OnPrimitiveCreate(GameObject _gameObject) {
        MinimapDisplay.AddRouteMarker(_gameObject.transform);
       ASLObjectTrackingSystem.AddObjectToTrack(_gameObject.GetComponent<ASLObject>(), _gameObject.transform);
    }
}
