using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleLine : MonoBehaviour
{
    public static ScaleLine _sl;
    public RectTransform myCanvas, RenderPanel;
    public LineRenderer centerLine, baseLine, branchM, branchF;

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
        Debug.Assert(centerLine != null);
        Debug.Assert(baseLine != null);
        Debug.Assert(branchM != null);
        Debug.Assert(branchF != null);
        StartCoroutine(UpdateScaleLine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator UpdateScaleLine()
    {
        Debug.Log(myCanvas.rect.height / 3f);
        Debug.Log(myCanvas.rect.height *.3f);
        while (true)
        {
            //get height of canvas
            float canvasHeight = myCanvas.rect.height;
            //get height, width of line panel
            float panelHeight = canvasHeight / 3f;
            float panelWidth = 50f;
            float panelX = 30f;
            float panelY = canvasHeight * 0.3f;
            //get center, top/bottom of line panel
            RenderPanel.rect.Set(panelX, panelY, panelWidth, panelHeight);

            //get positions, convert
            //Vector2 topPos = new Vector2(panelX + 0.5f * panelWidth, panelY);
            //Vector2 botPos = new Vector2(panelX + 0.5f * panelWidth, panelY + panelHeight);
            if(curDisplay != null)
            {
                Vector3 topPos = curDisplay.ScreenToWorldPoint(new Vector2(panelX + 0.5f * panelWidth, panelY));
                Vector3 botPos = curDisplay.ScreenToWorldPoint(new Vector2(panelX + 0.5f * panelWidth, panelY + panelHeight));
                Vector3[] cPosArr = { topPos, botPos };
                Debug.Log("" + cPosArr[0] + "," + cPosArr[1]);
                centerLine.SetPositions(cPosArr);
            }
            


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

                //get scale between canvas height and panel height
                //draw line from top to bottom edge of line panel, through center
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
        //mask to ignore markers and obstacles
        RaycastHit hitM;
        RaycastHit hitC;
        int castInfo = 0;

        if (closestChunk != null)
        {            
            if (mChunk.gameObject.Equals(closestChunk.gameObject)) {
                //linecast between camera and closest chunk
                //if we do not hit the chunk, our closest chunk is now null
                if(Physics.Linecast(curDisplay.transform.position, closestChunk.gameObject.transform.position, out hitC))
                {
                    if (closestChunk.gameObject.Equals(hitC.collider.gameObject)) return;
                    closestChunk = null;
                }
            }
            else
            {
                //linecast between camera and each chunk
                //TODO: raycast to closest points
                if (Physics.Linecast(curDisplay.transform.position, closestChunk.gameObject.transform.position, out hitC))
                {
                    if (closestChunk.gameObject.Equals(hitC.collider.gameObject))  castInfo++;
                }
                if (Physics.Linecast(curDisplay.transform.position, mChunk.gameObject.transform.position, out hitM))
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
            if (Physics.Linecast(curDisplay.transform.position, mChunk.gameObject.transform.position, out hitM))
            {
                if (mChunk.gameObject.Equals(hitM.collider.gameObject)) closestChunk = mChunk;
            }            
        }
    }

    public static void CheckIfInView(Renderer _r)
    {

    }
}
