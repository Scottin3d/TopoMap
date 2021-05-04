using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseProjector : MonoBehaviour
{

    bool isProjecting = true;
    bool onNormal = false;
    public GameObject mouseProjector = null;

    public GameObject point = null;

    private void Start() {
        //Debug.Assert(mouseProjector != null);
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.M)) {
            isProjecting = !isProjecting;
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            onNormal = !onNormal;
        }

        mouseProjector.SetActive(isProjecting);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.CompareTag("Chunk")) {
                mouseProjector.transform.position = hit.point;
                mouseProjector.transform.up = Vector3.up;
                if (onNormal) {
                    mouseProjector.transform.up = hit.normal;
                }
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(ray, out hit, ~LayerMask.NameToLayer("Marker"))) {
                if (hit.collider.CompareTag("Chunk")) {
                    GameObject p = Instantiate(point, hit.point, Quaternion.identity);
                }
            }
        }
    }
}
