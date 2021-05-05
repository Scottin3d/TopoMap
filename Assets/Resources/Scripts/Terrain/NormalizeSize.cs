using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalizeSize : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Normalize();
    }

    private void Normalize() {
        int width = GetComponent<Renderer>().material.mainTexture.width;
        int height = GetComponent<Renderer>().material.mainTexture.height;

        Vector3 scale = new Vector3(width / 1000f, 1f, height / 1000f);
        transform.localScale = scale;
    }

}
