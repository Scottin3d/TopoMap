using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleLine : MonoBehaviour
{
    public static ScaleLine _sl;
    public Canvas myCanvas;
    public RectTransform RenderPanel, RenderBase, RenderM, RenderF;
    public Text LabelM, LabelF;

    private static Camera curDisplay;
    private static ChunkData closestChunk;

    void Awake()
    {
        if (_sl == null) _sl = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(myCanvas != null);
        Debug.Assert(RenderPanel != null);
        Debug.Assert(RenderBase != null);
        Debug.Assert(RenderM != null);
        Debug.Assert(RenderF != null);
        StartCoroutine(UpdateScaleLine());
    }

    IEnumerator UpdateScaleLine()
    {
        while (true)
        {
            //get height of canvas
            float canvasHeight = myCanvas.pixelRect.height;
            //get height, width of line panel
            float panelHeight = canvasHeight * 0.3f; float panelWidth = 2f;
            float scaleWidth = 24f; float scaleHeight = 2f;
            float baseWidth = scaleWidth * 2f + panelWidth;

            float panelX = 50f; float panelY = canvasHeight * 0.3f;
            //get center, top/bottom of line panel
            RenderPanel.anchoredPosition = new Vector2(panelX, panelY);
            RenderPanel.sizeDelta = new Vector2(panelWidth, panelHeight);

            RenderBase.anchoredPosition = new Vector2(panelX, panelY - 0.5f * panelHeight);
            RenderBase.sizeDelta = new Vector2(baseWidth, scaleHeight);

            //Debug.Log(closestChunk);
            if (closestChunk != null)
            {
                //Debug.Log("Name: " + closestChunk.gameObject.name + " with parent: " + closestChunk.gameObject.transform.parent);
                //get parent of chunk
                Transform chunkParent = closestChunk.transform.parent;
                //get heightmap size
                float mapSize = -1f;
                if (chunkParent != null)
                {
                    mapSize = chunkParent.gameObject.GetComponent<GenerateMapFromHeightMap>().mapSize;
                }

                float mf_ScaleOffset = 0.25f * 3.28084f - 0.5f;
                RenderF.anchoredPosition = new Vector2(panelX - 0.5f * (scaleWidth + panelWidth), panelY + mf_ScaleOffset * panelHeight);
                RenderF.sizeDelta = new Vector2(scaleWidth, scaleHeight);

                RenderM.anchoredPosition = new Vector2(panelX + 0.5f * (scaleWidth + panelWidth), panelY - 0.25f * panelHeight);
                RenderM.sizeDelta = new Vector2(scaleWidth, scaleHeight);
            } else
            {
                RenderM.anchoredPosition = new Vector2(panelX - 0.5f * (scaleWidth + panelWidth), panelY - 0.5f * panelHeight);
                RenderM.sizeDelta = new Vector2(scaleWidth, scaleHeight);

                RenderF.anchoredPosition = new Vector2(panelX + 0.5f * (scaleWidth + panelWidth), panelY - 0.5f * panelHeight);
                RenderF.sizeDelta = new Vector2(scaleWidth, scaleHeight);
            }
            yield return new WaitForSeconds(0.01f);
        }        
    }

    public static void CheckDisplay(Camera _c)
    {
        curDisplay = _c;
    }

    public static void IsChunkVisible(ChunkData mChunk)
    {
        if (curDisplay == null) return;
        if (mChunk == null) return;
        //mask to ignore markers and obstacles https://answers.unity.com/questions/8715/how-do-i-use-layermasks.html
        int groundMask = LayerMask.NameToLayer("Ground");
        int holoMask = LayerMask.NameToLayer("Holomap");
        int mask1 = 1 << groundMask; int mask2 = 1 << holoMask;
        int combMask = mask1 | mask2;
        
        RaycastHit hitM;
        RaycastHit hitC;
        int castInfo = 0;

        if (closestChunk != null)
        {            
            if (mChunk.gameObject.Equals(closestChunk.gameObject)) {
                //linecast between camera and closest chunk
                //if we do not hit the chunk, our closest chunk is now null
                if(Physics.Linecast(curDisplay.transform.position, closestChunk.gameObject.transform.position, out hitC, combMask))
                {
                    if (closestChunk.gameObject.Equals(hitC.collider.gameObject)) return;
                    closestChunk = null;
                }
            }
            else
            {
                //linecast between camera and each chunk
                //TODO: raycast to closest points
                if (Physics.Linecast(curDisplay.transform.position, closestChunk.gameObject.transform.position, out hitC, combMask))
                {
                    if (closestChunk.gameObject.Equals(hitC.collider.gameObject))  castInfo++;
                }
                if (Physics.Linecast(curDisplay.transform.position, mChunk.gameObject.transform.position, out hitM, combMask))
                {
                    if (mChunk.gameObject.Equals(hitM.collider.gameObject)) castInfo += 2;
                }

                //if we do not hit either chunk, our closest chunk is now null
                //if we hit 1 chunk, out closest chunk is now that chunk
                //if we hit 2 chunks, determine the closest chunk
                switch (castInfo)
                {                    
                    case 3: closestChunk = (hitC.distance >= hitM.distance) ? closestChunk : mChunk;
                        break;
                    case 2: closestChunk = mChunk;
                        break;
                    case 1: //Do nothing
                        break;
                    default: closestChunk = null;
                        break;
                }
                
            }

        } else
        {
            //linecast between camera and chunk
            //if we do not hit the chunk, do nothing
            //otherwise, our new closest chunk is the chunk
            if (Physics.Linecast(curDisplay.transform.position, mChunk.gameObject.transform.position, out hitM, combMask))
            {
                if (mChunk.gameObject.Equals(hitM.collider.gameObject)) {
                    closestChunk = mChunk;
                    Debug.Log(hitM.collider.gameObject);
                } 
            }            
        }
    }

    public static void CheckIfInView(Renderer _r)
    {

    }
}
