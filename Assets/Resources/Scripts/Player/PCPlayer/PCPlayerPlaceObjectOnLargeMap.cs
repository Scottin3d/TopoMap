using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCPlayerPlaceObjectOnLargeMap : MonoBehaviour
{
    public GameObject PlayerLargePlaceFunction;

    public GameObject PlayerTeleportObject;
    public Camera PlayerCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AbleOrDisablePCRawImageUI();

        if (!PlayerTeleportObject.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            SwitchImage();
            InstantiateLargeMapProduct();
        }
    }

    private void AbleOrDisablePCRawImageUI()
    {
        if (PlayerTeleportObject.GetComponent<TeleportBetweenMap>().GetAtSmallMap())
        {
            PlayerLargePlaceFunction.GetComponent<ProductImageLoader>().PC_SetRawImageInactiveWhenAtSmallMap();
        }
        else
        {
            PlayerLargePlaceFunction.GetComponent<ProductImageLoader>().PC_SetRawImageActiveWhenAtSmallMap();
        }
    }

    private void InstantiateLargeMapProduct()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GetRaycastHitPoint();
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
                List<string> PCPathList = PlayerLargePlaceFunction.GetComponent<ProductImageLoader>().GetPathList();
                int CurPathListIndex = PlayerLargePlaceFunction.GetComponent<ProductImageLoader>().GetPCPathListIndex();

                string Path = PCPathList[CurPathListIndex];
                ASL.ASLHelper.InstantiateASLObject(Path, Hit.point, Quaternion.identity, "", "", GetCreatedProject);
            }
        }
    }

    private static void GetCreatedProject(GameObject _myGameObject)
    {

    }

    private void SwitchImage()
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            PlayerLargePlaceFunction.GetComponent<ProductImageLoader>().PC_SwitchToNextImage();
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            PlayerLargePlaceFunction.GetComponent<ProductImageLoader>().PC_SwitchToPreviousImage();
        }
    }
}
