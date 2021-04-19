using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseProjector : MonoBehaviour
{

    bool isProjecting = false;
    public GameObject mouseProjector = null;

    public GameObject point = null;
    public int segments;
    public float xradius;
    public float yradius;
    LineRenderer line;

    private void Start() {
        Debug.Assert(mouseProjector != null);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.M)) {
            isProjecting = !isProjecting;
        }
        mouseProjector.SetActive(isProjecting);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.CompareTag("Chunk")) {
                mouseProjector.transform.position = hit.point;
                mouseProjector.transform.up = hit.normal;
            }
        }

        if (Input.GetMouseButtonDown(0) && hit.transform) {
            GameObject p = Instantiate(point);
            p.transform.position = hit.point;
            //p.transform.up = hit.normal;
        }


    }

    void CreatePoints() {
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            line.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }
}
