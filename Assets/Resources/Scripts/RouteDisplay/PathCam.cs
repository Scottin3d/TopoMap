using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathCam : MonoBehaviour
{
    private Camera cam;
    private bool IsRendering = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Debug.Assert(cam != null, "Missing path camera component.");
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null)
        {
            IsRendering = false;
        } 

        if(cam != null) cam.enabled = IsRendering;
        if (!IsRendering) DeactivatePathCam();
    }

    public void SetRender(bool toRender) { IsRendering = toRender; }

    private static void DeactivatePathCam()
    {
        GameObject _pathTex = GameObject.FindWithTag("PathCam");
        RawImage raw = (_pathTex != null) ? _pathTex.GetComponent<RawImage>() : null;
        RenderTexture _render = (raw != null) ? raw.texture as RenderTexture : null;

        if (_render != null)
        {
            //From https://forum.unity.com/threads/how-to-clear-a-render-texture-to-transparent-color-all-bytes-at-0.147431/
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = _render;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }
    }
}
