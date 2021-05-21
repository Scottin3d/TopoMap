using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductImageLoader : MonoBehaviour
{
    public RawImage PCRawImage;
    public RawImage VRRawImage;

    public GameObject PlayerTeleportObject;
    public Camera PlayerCamera;

    public Texture2D BuildingImage;
    public GameObject BuildingPrefab;
    private string BuildingPathString = "LargeMapProduct/Building/rpgpp_lt_building_01";

    public Texture2D Bush;
    public GameObject BushPrefab;
    private string BushPathString = "LargeMapProduct/Bushes/rpgpp_lt_bush_01";

    public Texture2D TreeOne;
    public GameObject TreeOnePrefab;
    private string TreeOnePathString = "LargeMapProduct/Trees/rpgpp_lt_tree_01";

    public Texture2D TreeTwo;
    public GameObject TreeTwoPrefab;
    private string TreeTwoPathString = "LargeMapProduct/Trees/rpgpp_lt_tree_02";

    public Texture2D TreeThree;
    public GameObject TreeThreePrefab;
    private string TreeThreePathString = "LargeMapProduct/Trees/rpgpp_lt_tree_pine_01";

    private List<Texture2D> ImageList = new List<Texture2D>();
    private List<GameObject> PrefabList = new List<GameObject>();
    private List<string> PathList = new List<string>();

    private int PCImageListIndex = 0;
    private int PCPrefabListIndex = 0;
    private int PCPathListIndex = 0;

    private int VRImageListIndex = 0;
    private int VRPrefabListIndex = 0;
    private int VRPathListIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        AddImage_PrefabList();
        PCRawImage.GetComponent<RawImage>().texture = ImageList[PCImageListIndex];
        VRRawImage.GetComponent<RawImage>().texture = ImageList[VRImageListIndex];

        //if (PlayerTeleportObject.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        //{
        //    PCRawImage.enabled = false;
        //}
        //else
        //{
        //    PCRawImage.enabled = true;
        //}

        //Set both Raw Image to inactive, because player initially spawn at small map
        PCRawImage.enabled = false;
        VRRawImage.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //AbleOrDisableRawImageUI();

        //if (!PlayerTeleportObject.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        //{
        //    SwitchImage();
        //    InstantiateLargeMapProduct();
        //}
    }

    public void PC_SetRawImageInactiveWhenAtSmallMap()
    {
        PCRawImage.enabled = false;
    }

    public void VR_SetRawImageInactiveWhenAtSmallMap()
    {
        VRRawImage.enabled = false;
    }

    public void PC_SetRawImageActiveWhenAtSmallMap()
    {
        PCRawImage.enabled = true;
    }

    public void VR_SetRawImageActiveWhenAtSmallMap()
    {
        VRRawImage.enabled = true;
    }

    public List<string> GetPathList()
    {
        return PathList;
    }

    public int GetPCPathListIndex()
    {
        return PCPathListIndex;
    }

    public int GetVRPathListIndex()
    {
        return VRPathListIndex;
    }

    public void PC_SwitchToNextImage()
    {
        if (PCImageListIndex == ImageList.Count - 1)
        {
            PCImageListIndex = 0;
            PCPathListIndex = 0;
        }
        else
        {
            PCImageListIndex++;
            PCPathListIndex++;
        }

        PCRawImage.GetComponent<RawImage>().texture = ImageList[PCImageListIndex];
    }

    public void VR_SwitchToNextImage()
    {
        if (VRImageListIndex == ImageList.Count - 1)
        {
            VRImageListIndex = 0;
            VRPathListIndex = 0;
        }
        else
        {
            VRImageListIndex++;
            VRPathListIndex++;
        }

        VRRawImage.GetComponent<RawImage>().texture = ImageList[VRImageListIndex];
    }

    public void PC_SwitchToPreviousImage()
    {
        if (PCImageListIndex == 0)
        {
            PCImageListIndex = ImageList.Count - 1;
            PCPathListIndex = ImageList.Count - 1;
        }
        else
        {
            PCImageListIndex--;
            PCPathListIndex--;
        }

        PCRawImage.GetComponent<RawImage>().texture = ImageList[PCImageListIndex];
    }

    public void VR_SwitchToPreviousImage()
    {
        if (VRImageListIndex == 0)
        {
            VRImageListIndex = ImageList.Count - 1;
            VRPathListIndex = ImageList.Count - 1;
        }
        else
        {
            VRImageListIndex--;
            VRPathListIndex--;
        }

        VRRawImage.GetComponent<RawImage>().texture = ImageList[VRImageListIndex];
    }

    private void AddImage_PrefabList()
    {
        ImageList.Add(BuildingImage);
        ImageList.Add(Bush);
        ImageList.Add(TreeOne);
        ImageList.Add(TreeTwo);
        ImageList.Add(TreeThree);

        PrefabList.Add(BuildingPrefab);
        PrefabList.Add(BushPrefab);
        PrefabList.Add(TreeOnePrefab);
        PrefabList.Add(TreeTwoPrefab);
        PrefabList.Add(TreeThreePrefab);

        PathList.Add(BuildingPathString);
        PathList.Add(BushPathString);
        PathList.Add(TreeOnePathString);
        PathList.Add(TreeTwoPathString);
        PathList.Add(TreeThreePathString);
    }

    private void SwitchImage()
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (PCImageListIndex == ImageList.Count - 1)
            {
                PCImageListIndex = 0;
                PCPrefabListIndex = 0;
                PCPathListIndex = 0;
            }
            else
            {
                PCImageListIndex++;
                PCPrefabListIndex++;
                PCPathListIndex++;
            }

            PCRawImage.GetComponent<RawImage>().texture = ImageList[PCImageListIndex];
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (PCImageListIndex == 0)
            {
                PCImageListIndex = ImageList.Count - 1;
                PCPrefabListIndex = ImageList.Count - 1;
                PCPathListIndex = ImageList.Count - 1;
            }
            else
            {
                PCImageListIndex--;
                PCPrefabListIndex--;
                PCPathListIndex--;
            }

            PCRawImage.GetComponent<RawImage>().texture = ImageList[PCImageListIndex];
        }
    }

    //private void InstantiateLargeMapProduct()
    //{
    //    if (Input.GetKeyDown(KeyCode.Z))
    //    {
    //        GetRaycastHitPoint();
    //    }
    //}

    //private void AbleOrDisableRawImageUI()
    //{
    //    if (PlayerTeleportObject.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
    //    {
    //        PCRawImage.enabled = false;
    //    }
    //    else
    //    {
    //        PCRawImage.enabled = true;
    //    }
    //}

    //private void GetRaycastHitPoint()
    //{
    //    Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit Hit;

    //    if (Physics.Raycast(MouseRay, out Hit))
    //    {
    //        if (Hit.collider.tag == "Chunk")
    //        {
    //            string Path = PathList[PCPathListIndex];
    //            ASL.ASLHelper.InstantiateASLObject(Path, Hit.point, Quaternion.identity, "", "", GetCreatedProject);
    //        }
    //    }
    //}

    //private static void GetCreatedProject(GameObject _myGameObject)
    //{

    //}
}
