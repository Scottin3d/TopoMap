using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSystem : MonoBehaviour
{
    public GameObject BigMap;

    private List<GameObject> MyCloudList = new List<GameObject>();

    //Instantiate 4 Clouds in 100 square units flat form
    void Start()
    {
        int XZ_Range = BigMap.GetComponent<GenerateMapFromHeightMap>().mapSize / 2;
        int YHeight = 150;

        //for (int x = 0; x <= XZ_Range; x += 10)
        //{
        //    for (int z = 0; z < XZ_Range; z += 10)
        //    {

        //    }
        //}

        for (int i = 0; i < 400; i++)
        {
            Vector3 NewPosition = GetRandomPosition(XZ_Range, YHeight);

            if (i % 2 == 0)
            {
                GameObject Cloud = Instantiate(Resources.Load("MyPrefabs/LargeMapProduct/Sky/rpgpp_lt_cloud_01") as GameObject);
                Cloud.transform.position = NewPosition;
                Cloud.transform.parent = this.transform;
                MyCloudList.Add(Cloud);
            }
            else
            {
                GameObject Cloud = Instantiate(Resources.Load("MyPrefabs/LargeMapProduct/Sky/rpgpp_lt_cloud_02") as GameObject);
                Cloud.transform.position = NewPosition;
                Cloud.transform.parent = this.transform;
                MyCloudList.Add(Cloud);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 GetRandomPosition(int XZRange, int YHeight)
    {
        Vector3 RandomPosition = new Vector3(Random.Range((XZRange + 100) * -1, XZRange + 100), Random.Range(YHeight - 30, YHeight + 30), Random.Range((XZRange + 100) * -1, XZRange + 100));
        return RandomPosition;
    }
}
