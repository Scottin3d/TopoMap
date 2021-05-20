using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductImageLoader : MonoBehaviour
{
    public RawImage MyRawImage;

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

    private int ImageListIndex = 0;
    private int PrefabListIndex = 0;
    private int PathListIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        AddImage_PrefabList();
        MyRawImage.GetComponent<RawImage>().texture = ImageList[ImageListIndex];

        if (PlayerTeleportObject.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            MyRawImage.enabled = false;
        }
        else
        {
            MyRawImage.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        AbleOrDisableRawImageUI();

        if (!PlayerTeleportObject.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            SwitchImage();
            InstantiateLargeMapProduct();
        }
    }

    private void InstantiateLargeMapProduct()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GetRaycastHitPoint();
        }
    }

    private void SwitchImage()
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (ImageListIndex == ImageList.Count - 1)
            {
                ImageListIndex = 0;
                PrefabListIndex = 0;
                PathListIndex = 0;
            }
            else
            {
                ImageListIndex++;
                PrefabListIndex++;
                PathListIndex++;
            }

            MyRawImage.GetComponent<RawImage>().texture = ImageList[ImageListIndex];
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (ImageListIndex == 0)
            {
                ImageListIndex = ImageList.Count - 1;
                PrefabListIndex = ImageList.Count - 1;
                PathListIndex = ImageList.Count - 1;
            }
            else
            {
                ImageListIndex--;
                PrefabListIndex--;
                PathListIndex--;
            }

            MyRawImage.GetComponent<RawImage>().texture = ImageList[ImageListIndex];
        }
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
    
    private void AbleOrDisableRawImageUI()
    {
        if (PlayerTeleportObject.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            MyRawImage.enabled = false;
        }
        else
        {
            MyRawImage.enabled = true;
        }
    }

    private void GetRaycastHitPoint()
    {
        Ray MouseRay = PlayerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit Hit;

        if (Physics.Raycast(MouseRay, out Hit))
        {
            if (Hit.collider.tag == "Chunk")
            {
                string Path = PathList[PathListIndex];
                ASL.ASLHelper.InstantiateASLObject(Path, Hit.point, Quaternion.identity, "", "", GetCreatedProject);
            }
        }
    }

    private static void GetCreatedProject(GameObject _myGameObject)
    {

    }
}
