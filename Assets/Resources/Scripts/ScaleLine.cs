using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleLine : MonoBehaviour
{
    public static ScaleLine _sl;
    public Canvas myCanvas;
    public RectTransform RenderPanel, RenderBase, RenderM, RenderF; //(255,255,255), (0,0,150), (0,150,0), (150,0,0)
    public Text LabelM, LabelF;
    private RectTransform LabelM_Rect, LabelF_Rect;

    private static Camera curDisplay;
    private static ChunkData closestChunk;
    private static bool IsValidUpdate = false;

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
        Debug.Assert(LabelF != null);
        Debug.Assert(LabelM != null);
        LabelM_Rect = LabelM.GetComponent<RectTransform>();
        LabelF_Rect = LabelF.GetComponent<RectTransform>();
        StartCoroutine(UpdateScaleLine());
    }

    /// <summary>
    /// Updates the scale line GUI widget. 
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateScaleLine()
    {
        while (true)
        {
            IsValidUpdate = false;
            bool OnSmallMap = false;

            //get height of canvas
            float canvasHeight = myCanvas.pixelRect.height;
            //get height, width of line panel
            float panelHeight = canvasHeight * 0.3f; float panelWidth = 2f;
            float scaleWidth = 18f; float scaleHeight = 2f;
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
                //get parent of chunk
                Transform chunkParent = closestChunk.transform.parent;
                //get chunk size
                float chunkSize = -1f; float meshHeight = -1f;
                if (chunkParent != null)
                {
                    chunkSize = chunkParent.gameObject.GetComponent<GenerateMapFromHeightMap>().ChunkSize;
                    meshHeight = chunkParent.gameObject.GetComponent<GenerateMapFromHeightMap>().meshHeight;
                    if (chunkParent.gameObject.tag == "SpawnSmallMap") { 
                        //chunkSize = chunkSize * MarkerDisplay.GetScaleFactor();
                        OnSmallMap = true;
                    } else { OnSmallMap = false; }
                    
                }
                if(chunkSize > 0)
                {
                    if (OnSmallMap)
                    {
                        //horizontal distance
                        //get positions on opposite ends of the chunk (center y is y of parent) (+- 0.5f * chunkSize * Vector3.forward)
                        Vector3 topPos = new Vector3(closestChunk.MapChunk.center.x, chunkParent.position.y, closestChunk.MapChunk.center.y) + 0.5f * chunkSize * Vector3.forward;
                        Vector3 botPos = new Vector3(closestChunk.MapChunk.center.x, chunkParent.position.y, closestChunk.MapChunk.center.y) - 0.5f * chunkSize * Vector3.forward;
                        //convert world positions to screen positions 
                        Vector3 screenTop = curDisplay.WorldToScreenPoint(topPos);
                        Vector3 screenBot = curDisplay.WorldToScreenPoint(botPos);
                        //get distance between screen positions
                        float chunkScreenSize = (screenBot - screenTop).magnitude;
                        //compare distance to total height of scale line (this is meter distance)
                        float scaleFactor = 1f;
                        while (chunkScreenSize > canvasHeight) { chunkScreenSize /= 2f; scaleFactor /= 2f; }
                        float tempScreenSize = chunkScreenSize * 2f;
                        if(tempScreenSize > 0f)
                        {
                            while (tempScreenSize < panelHeight && tempScreenSize != 0f)
                            {
                                scaleFactor *= 2f;
                                tempScreenSize *= 2f;
                            }
                            chunkScreenSize *= scaleFactor;
                            float displaySizeM = chunkSize * MarkerDisplay.GetScaleFactor() * scaleFactor;
                            float displaySizeF = displaySizeM / 3.28084f;   //divide by 3.28084(this is ft distance)
                                                                            //Debug.Log((chunkScreenSize / panelHeight) + " vs " + ((chunkScreenSize / panelHeight) / 3.28084f));

                            RenderM.anchoredPosition = new Vector2(panelX - 0.5f * (scaleWidth + panelWidth), panelY + (chunkScreenSize / panelHeight - 0.5f) * panelHeight);
                            RenderF.anchoredPosition = new Vector2(panelX + 0.5f * (scaleWidth + panelWidth), panelY + ((chunkScreenSize / panelHeight) / 3.28084f - 0.5f) * panelHeight);
                            LabelM_Rect.anchoredPosition = new Vector2(RenderM.anchoredPosition.x + 10f, RenderM.anchoredPosition.y + 20f);
                            LabelF_Rect.anchoredPosition = new Vector2(RenderF.anchoredPosition.x + 10f, RenderF.anchoredPosition.y + 20f);
                            LabelM.text = String.Format("{0}m", displaySizeM);
                            LabelF.text = String.Format("{0}ft.", displaySizeF);
                            IsValidUpdate = true;
                        } 
                        
                    }   
                    else
                    {
                        if(meshHeight > 0)
                        {

                            //Get positions corresponding to mesh height
                            Vector3 forwardPos = curDisplay.transform.position + curDisplay.transform.forward;
                            Vector3 topPos = forwardPos + curDisplay.transform.up * meshHeight;
                            //convert world positions to screen positions 
                            Vector3 screenTop = curDisplay.WorldToScreenPoint(topPos);
                            Vector3 screenBot = curDisplay.WorldToScreenPoint(forwardPos);
                            //get distance between screen positions
                            float chunkScreenSize = (screenBot - screenTop).magnitude;
                            //compare distance to total height of scale line (this is meter distance)
                            float scaleFactor = 1f;
                            
                            while (chunkScreenSize > canvasHeight) { chunkScreenSize /= 2f; /*scaleFactor /= 2f;*/ } 
                            
                            if(chunkScreenSize > 0f)
                            {
                                float tempScreenSize = chunkScreenSize;
                                if(tempScreenSize > panelHeight)
                                {
                                    while (tempScreenSize > panelHeight)
                                    {
                                        scaleFactor /= 2f;
                                        tempScreenSize /= 2f;
                                    }
                                } else if(tempScreenSize < panelHeight)
                                {
                                    while (tempScreenSize < panelHeight)
                                    {
                                        scaleFactor *= 2f;
                                        tempScreenSize *= 2f;
                                    }
                                }
                                //Debug.Log(panelHeight + " vs " + chunkScreenSize);
                                chunkScreenSize *= scaleFactor;
                                float displaySizeM = meshHeight * scaleFactor;
                                float displaySizeF = displaySizeM / 3.28084f;

                                RenderM.anchoredPosition = new Vector2(panelX - 0.5f * (scaleWidth + panelWidth), panelY + (chunkScreenSize / panelHeight - 0.5f) * panelHeight);
                                RenderF.anchoredPosition = new Vector2(panelX + 0.5f * (scaleWidth + panelWidth), panelY + ((chunkScreenSize / panelHeight) / 3.28084f - 0.5f) * panelHeight);
                                LabelM_Rect.anchoredPosition = new Vector2(RenderM.anchoredPosition.x + 10f, RenderM.anchoredPosition.y + 20f);
                                LabelF_Rect.anchoredPosition = new Vector2(RenderF.anchoredPosition.x + 10f, RenderF.anchoredPosition.y + 20f);
                                LabelM.text = String.Format("{0}m", displaySizeM);
                                LabelF.text = String.Format("{0}ft.", displaySizeF);
                                IsValidUpdate = true;
                            }                             
                        }                         
                    }
                    //float mf_ScaleOffset = 0.25f * 3.28084f - 0.5f;
                }               
            } 
            if (!IsValidUpdate)
            {
                RenderM.anchoredPosition = new Vector2(panelX - 0.5f * (scaleWidth + panelWidth), panelY - 0.5f * panelHeight);
                RenderF.anchoredPosition = new Vector2(panelX + 0.5f * (scaleWidth + panelWidth), panelY - 0.5f * panelHeight);
                LabelM.text = "";
                LabelF.text = "";
            }
            RenderF.sizeDelta = new Vector2(scaleWidth, scaleHeight);
            RenderM.sizeDelta = new Vector2(scaleWidth, scaleHeight);
            yield return new WaitForSeconds(0.01f);
        }        
    }

    /// <summary>
    /// Sets the display camera used for calculations and visibility checking
    /// </summary>
    /// <param name="_c">The camera to use as the new display</param>
    public static void CheckDisplay(Camera _c)
    {
        curDisplay = _c;
    }

    /// <summary>
    /// Tests if a MapChunk would be "visible" within some distance of the current display camera
    /// </summary>
    /// <param name="mChunk">The MapChunk data to use in testing</param>
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
                    //Debug.Log(hitM.collider.gameObject);
                } 
            }            
        }
    }
}
