using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GenerateMapFromHeightMap : MonoBehaviour {
    [Header("Terrain Generation Demo")]
    public bool demoMode = false;
    [Range(0.01f, 1f)]
    public float speed = 0.2f;
    private float timeStep;
    [SerializeField]
    List<GameObject> demoChunks = new List<GameObject>();

    public Camera cam = null;
    private Vector3 defaultCamPos;
    private void Update() {
        timeStep = 1f - speed;
    }
    private void ChangeChunkColor() {
        foreach (var c in demoChunks) {
            c.GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

    private void GenDemo() {
        DemoTextUpdater.current.heightmapRes.text = heightmap.width.ToString() + "px, " + heightmap.height.ToString() + "px";
        DemoTextUpdater.current.chunkRes.text = chunkResolution.ToString();
        DemoTextUpdater.current.numberOfChunks.text = numberOfChunks.ToString();
        defaultCamPos = cam.transform.position;
        // 1. generate map chunks
        StartCoroutine(GenerateMapC());

    }


    IEnumerator GenerateMapC() {
        DemoTextUpdater.current.Progress("Generating MapData...");
        speed = 0.6f;
        yield return timeStep;
        mapChunks = new MapChunk[numberOfChunks, numberOfChunks];   // set map chunk container

        int mapWidth = heightmap.width;                             // full heightmap resolution, min 32
        int mapHeight = heightmap.height;                           // full heightmap resolution, min 32

        float mapLowerLeftX = mapSize / -2f;                        // constructing the map from left -> right, bottom -> top
        float mapLowerLeftZ = mapSize / -2f;

        // 1. generate map chunks
        for (int z = 0; z < numberOfChunks; z++) {
            for (int x = 0; x < numberOfChunks; x++) {
                // create demo chunk
                DemoTextUpdater.current.miscText.text = "Creating GameObject...\n";
                GameObject demoChunk = GameObject.CreatePrimitive(PrimitiveType.Plane);
                demoChunk.name = "Chunk(" + x + ", " + z + ")";

                DemoTextUpdater.current.Chunk(demoChunk.name);

                demoChunks.Add(demoChunk);
                // find the center of the chunk
                float halfChunk = chunkSize / 2f;
                Vector2 _chunkCenter = new Vector2(transform.position.x + mapLowerLeftX + (x * chunkSize) + halfChunk,
                                                 transform.position.z + mapLowerLeftZ + (z * chunkSize) + halfChunk);
                DemoTextUpdater.current.miscText.text += "Finding Center...\n";
                // set demo chunk position
                demoChunk.transform.position = new Vector3(_chunkCenter.x, 5.2f, _chunkCenter.y);
                demoChunk.transform.localScale = new Vector3(chunkSize, 1, chunkSize) / mapSize;
                Quaternion rotation = Quaternion.identity;
                rotation.eulerAngles = new Vector3(0f, 180f, 0f);
                demoChunk.transform.localRotation = rotation;

                DemoTextUpdater.current.miscText.text += "Fetching New Heightmap...\n";
                // generate heightmap chunk
                Texture2D _heightmap = TextureGenerator.GetPixelMap(heightmap, (mapWidth / numberOfChunks) * x,
                                                    (mapHeight / numberOfChunks) * z,
                                                    mapWidth / numberOfChunks);

                DemoTextUpdater.current.miscText.text += "Creating MapData...\n";
                // generate map data
                MapData _mapData = GenerateMapData(_heightmap);

                DemoTextUpdater.current.miscText.text += "Creating Map Chunk...\n";
                // create chunk
                mapChunks[x, z] = new MapChunk(_heightmap, _chunkCenter, _mapData);

                demoChunk.GetComponent<MeshRenderer>().material.mainTexture = _heightmap;
                ChangeChunkColor();
                yield return new WaitForSeconds(timeStep);
            }
        }

        // 2. find neighbors
        StartCoroutine(FindNeighborsC());

    }

    IEnumerator FindNeighborsC() {
        speed = 0.87f;

        DemoTextUpdater.current.Progress("Finding Neighbors...");
        for (int z = 0; z < numberOfChunks; z++) {
            for (int x = 0; x < numberOfChunks; x++) {
                DemoTextUpdater.current.Chunk("Chunk(" + x + ", " + z + ")");
                DemoTextUpdater.current.miscText.text = "";
                demoChunks[(z * numberOfChunks) + x].GetComponent<Renderer>().material.color = Color.green;
                yield return new WaitForSeconds(timeStep);
                //==sides==
                // Top : z + 1 <= resolution - 1
                if (z + 1 <= numberOfChunks - 1) {
                    DemoTextUpdater.current.miscText.text += "Top Neighbor...\n";
                    mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.Top] = mapChunks[x, z + 1];
                    int topIndex = ((z + 1) * numberOfChunks) + (x + 0);
                    demoChunks[topIndex].GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForSeconds(timeStep);
                    // check corners
                    // left
                    bool topLeft = (x - 1 >= 0);
                    if (topLeft) {
                        DemoTextUpdater.current.miscText.text += "Top Left Neighbor...\n";

                        mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.TopLeft] = mapChunks[x - 1, z + 1];
                        int topLeftindex = ((z + 1) * numberOfChunks) + (x - 1);
                        demoChunks[topLeftindex].GetComponent<Renderer>().material.color = Color.red;
                        yield return new WaitForSeconds(timeStep);
                    }
                    // right
                    bool topRight = (x + 1 <= numberOfChunks - 1);
                    if (topRight) {
                        DemoTextUpdater.current.miscText.text += "Top Right Neighbor...\n";

                        mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.TopRight] = mapChunks[x + 1, z + 1];
                        int topRightIndex = ((z + 1) * numberOfChunks) + (x + 1);
                        demoChunks[topRightIndex].GetComponent<Renderer>().material.color = Color.red;
                        yield return new WaitForSeconds(timeStep);
                    }
                }
                // Right : x + 1 <= resolution - 1
                if (x + 1 <= numberOfChunks - 1) {
                    DemoTextUpdater.current.miscText.text += "Right Neighbor...\n";

                    mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.Right] = mapChunks[x + 1, z];
                    int rightIndex = ((z + 0) * numberOfChunks) + (x + 1);
                    demoChunks[rightIndex].GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForSeconds(timeStep);
                }
                // Bottom : z - 1 >= 0
                if (z - 1 >= 0) {
                    DemoTextUpdater.current.miscText.text += "Bottom Neighbor...\n";

                    mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.Bottom] = mapChunks[x, z - 1];
                    int bottomIndex = ((z - 1) * numberOfChunks) + (x + 0);
                    demoChunks[bottomIndex].GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForSeconds(timeStep);
                    // check corners
                    // left
                    if (x - 1 >= 0) {
                        DemoTextUpdater.current.miscText.text += "Bottom left Neighbor...\n";

                        mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.TopLeft] = mapChunks[x - 1, z - 1];
                        int bottomLeftIndex = ((z - 1) * numberOfChunks) + (x - 1);
                        demoChunks[bottomLeftIndex].GetComponent<Renderer>().material.color = Color.red;
                        yield return new WaitForSeconds(timeStep);
                    }
                    // right
                    if (x + 1 <= numberOfChunks - 1) {
                        DemoTextUpdater.current.miscText.text += "Bottom Right Neighbor...\n";

                        mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.TopRight] = mapChunks[x + 1, z - 1];
                        int bottomRightIndex = ((z - 1) * numberOfChunks) + (x + 1);
                        demoChunks[bottomRightIndex].GetComponent<Renderer>().material.color = Color.red;
                        yield return new WaitForSeconds(timeStep);
                    }

                }
                // Left : x - 1 >= 0
                if (x - 1 >= 0) {
                    DemoTextUpdater.current.miscText.text += "left Neighbor...\n";

                    mapChunks[x, z].chunkNeighbors[(int)MapChunkNeighbor.Left] = mapChunks[x - 1, z];
                    int leftIndex = ((z + 0) * numberOfChunks) + (x - 1);
                    demoChunks[leftIndex].GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForSeconds(timeStep);
                }

                DemoTextUpdater.current.miscText.text += "Generating MeshData...\n";

                // generate mesh data
                MeshData _meshData = MeshGenerator.GenerateTerrainMesh(mapChunks[x, z], meshHeight, meshHieghtCurve,
                                                                       chunkSize, editorPreviewLOD, ref worldMaxHeight, ref worldMinHeight);
                mapChunks[x, z].meshData = _meshData;

                // reset colors
                ChangeChunkColor();
                yield return new WaitForSeconds(timeStep);
            }
        }

        // 4. set material min max values
        material.SetFloat("_WorldMax", worldMaxHeight);
        material.SetFloat("_WorldMin", worldMinHeight);

        // 3. smooth the edges
        StartCoroutine(SmoothChunkEdgesC());

    }

    IEnumerator SmoothChunkEdgesC() {
        speed = 0.95f;
        DemoTextUpdater.current.Progress("Smoothing Edges...");
        DemoTextUpdater.current.miscText.text = "";

        int row = 0;
        int col = 0;
        int chunkLength = chunkResolution;

        cam.orthographic = false;
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(80f, 0f, 0f);
        cam.transform.rotation = rotation;
        //cam.orthographicSize = 0.5f;

        for (int chunk = 0; chunk < numberOfChunks * numberOfChunks; chunk++) {
            // check and reset row
            if (row == numberOfChunks) {
                row = 0;
                col++;
            }
            DemoTextUpdater.current.miscText.text = "";
            DemoTextUpdater.current.Chunk("Chunk(" + row + ", " + col + ")");

            MapChunk currChunk = mapChunks[row, col];
            demoChunks[chunk].GetComponent<Renderer>().material.color = Color.green;
            cam.transform.position = new Vector3(currChunk.center.x, 8f, currChunk.center.y - 0.5f);
            yield return new WaitForEndOfFrame();

            // top
            //for (int i = 0; i < chunkLength; i++) {
            // variables

            MapChunk topNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Top];
            int topIndex = ((col + 1) * numberOfChunks) + (row + 0);
            MapChunk rightNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Right];
            int rightIndex = ((col + 0) * numberOfChunks) + (row + 1);
            MapChunk bottomNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Bottom];
            int bottomIndex = ((col - 1) * numberOfChunks) + (row + 0);
            MapChunk leftNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Left];
            int leftIndex = ((col + 0) * numberOfChunks) + (row - 1);

            List<GameObject> chunkTopVertices = new List<GameObject>();
            List<GameObject> chunkRightVertices = new List<GameObject>();
            List<GameObject> chunkBottomVertices = new List<GameObject>();
            List<GameObject> chunkLeftVertices = new List<GameObject>();


            List<GameObject> topVertices = new List<GameObject>();
            List<GameObject> rightVertices = new List<GameObject>();
            List<GameObject> bottomVertices = new List<GameObject>();
            List<GameObject> leftVertices = new List<GameObject>();

            // top
            if (topNeighbor != null) {
                DemoTextUpdater.current.miscText.text += "Smoothing Top...\n";

                for (int i = 0; i < chunkLength; i++) {
                    // generate chunk vert
                    GameObject vert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    vert.GetComponent<Renderer>().material.color = Color.green;
                    chunkTopVertices.Add(vert);
                    vert.transform.position = currChunk.meshData.vertices[currChunk.topVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y);
                    vert.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize) / mapSize / chunkResolution * 4;

                    // generate neighbor vert
                    GameObject nVert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    nVert.GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForEndOfFrame();
                    topVertices.Add(nVert);
                    nVert.transform.position = topNeighbor.meshData.vertices[topNeighbor.bottomVerts[i]] + new Vector3(topNeighbor.center.x, 5f, topNeighbor.center.y);
                    nVert.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize) / mapSize / chunkResolution * 4;

                    float yValue = currChunk.meshData.vertices[currChunk.topVerts[i]].y;
                    yValue += topNeighbor.meshData.vertices[topNeighbor.bottomVerts[i]].y;

                    topNeighbor.meshData.vertices[topNeighbor.bottomVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.topVerts[i]].y = yValue / 2f;

                    yield return new WaitForEndOfFrame();
                    // average value
                    Vector3 avg = vert.transform.position;
                    avg.y = yValue / 2f;

                    vert.transform.position = avg;

                    // normal
                    Vector3 normalBase = currChunk.meshData.vertices[currChunk.topVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y); ;
                    Vector3 normalTop = normalBase + (currChunk.meshData.normals[currChunk.topVerts[i]] * 1.1f);
                    Vector3 neighborTop = normalBase + (topNeighbor.meshData.normals[topNeighbor.bottomVerts[i]] * 1.1f);

                    Debug.DrawLine(normalBase, normalTop, Color.green, 2f);
                    Debug.DrawLine(normalBase, neighborTop, Color.red, 2f);


                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.topVerts[i]];
                    Vector3 neighborNormal = topNeighbor.meshData.normals[topNeighbor.bottomVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.topVerts[i]] = vertexNormal;
                    topNeighbor.meshData.normals[topNeighbor.bottomVerts[i]] = vertexNormal;

                    Vector3 avgNormal = normalBase + (vertexNormal * 1.1f);
                    Debug.DrawLine(normalBase, avgNormal, Color.blue, 5f);

                    yield return new WaitForEndOfFrame();
                }

                for (int i = 0; i < chunkResolution; i++) {
                    Destroy(chunkTopVertices[i]);
                    Destroy(topVertices[i]);
                }
            }

            // right
            if (rightNeighbor != null) {
                DemoTextUpdater.current.miscText.text += "Smoothing Right...\n";

                for (int i = 0; i < chunkLength; i++) {
                    // generate chunk vert
                    GameObject vert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    vert.GetComponent<Renderer>().material.color = Color.green;
                    chunkRightVertices.Add(vert);
                    vert.transform.position = currChunk.meshData.vertices[currChunk.rightVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y);
                    vert.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize) / mapSize / chunkResolution * 4;

                    // generate neighbor vert
                    GameObject nVert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    nVert.GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForEndOfFrame();
                    rightVertices.Add(nVert);
                    nVert.transform.position = rightNeighbor.meshData.vertices[rightNeighbor.leftVerts[i]] + new Vector3(rightNeighbor.center.x, 5f, rightNeighbor.center.y);
                    nVert.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize) / mapSize / chunkResolution * 4;

                    float yValue = currChunk.meshData.vertices[currChunk.rightVerts[i]].y;
                    yValue += rightNeighbor.meshData.vertices[rightNeighbor.leftVerts[i]].y;

                    rightNeighbor.meshData.vertices[rightNeighbor.leftVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.rightVerts[i]].y = yValue / 2f;
                    yield return new WaitForEndOfFrame();
                    // average value
                    Vector3 avg = vert.transform.position;
                    avg.y = yValue / 2f;

                    vert.transform.position = avg;

                    // normal
                    Vector3 normalBase = currChunk.meshData.vertices[currChunk.rightVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y); ;
                    Vector3 normalTop = normalBase + (currChunk.meshData.normals[currChunk.rightVerts[i]] * 2f);
                    Vector3 neighborTop = normalBase + (rightNeighbor.meshData.normals[rightNeighbor.leftVerts[i]] * 2f);

                    Debug.DrawLine(normalBase, normalTop, Color.green, 2f);
                    Debug.DrawLine(normalBase, neighborTop, Color.red, 2f);


                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.rightVerts[i]];
                    Vector3 neighborNormal = rightNeighbor.meshData.normals[rightNeighbor.leftVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.rightVerts[i]] = vertexNormal;
                    rightNeighbor.meshData.normals[rightNeighbor.leftVerts[i]] = vertexNormal;

                    Vector3 avgNormal = normalBase + (vertexNormal * 2f);
                    Debug.DrawLine(normalBase, avgNormal, Color.blue, 2f);


                    yield return new WaitForEndOfFrame();
                }
                for (int i = 0; i < chunkResolution; i++) {
                    Destroy(chunkRightVertices[i]);
                    Destroy(rightVertices[i]);
                }
            }

            // bottom
            if (bottomNeighbor != null) {
                DemoTextUpdater.current.miscText.text += "Smoothing Bottom...\n";

                for (int i = 0; i < chunkLength; i++) {
                    GameObject vert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    vert.GetComponent<Renderer>().material.color = Color.green;
                    chunkBottomVertices.Add(vert);
                    vert.transform.position = currChunk.meshData.vertices[currChunk.bottomVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y);
                    vert.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize) / mapSize / chunkResolution * 4;

                    // generate neighbor vert
                    GameObject nVert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    nVert.GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForEndOfFrame();
                    bottomVertices.Add(nVert);
                    nVert.transform.position = bottomNeighbor.meshData.vertices[bottomNeighbor.topVerts[i]] + new Vector3(bottomNeighbor.center.x, 5f, bottomNeighbor.center.y);
                    nVert.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize) / mapSize / chunkResolution * 4;

                    float yValue = currChunk.meshData.vertices[currChunk.bottomVerts[i]].y;
                    yValue += bottomNeighbor.meshData.vertices[bottomNeighbor.topVerts[i]].y;

                    bottomNeighbor.meshData.vertices[bottomNeighbor.topVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.bottomVerts[i]].y = yValue / 2f;
                    yield return new WaitForEndOfFrame();
                    // average value
                    Vector3 avg = vert.transform.position;
                    avg.y = yValue / 2f;

                    vert.transform.position = avg;

                    // normal
                    Vector3 normalBase = currChunk.meshData.vertices[currChunk.bottomVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y); ;
                    Vector3 normalTop = normalBase + (currChunk.meshData.normals[currChunk.bottomVerts[i]] * 2f);
                    Vector3 neighborTop = normalBase + (bottomNeighbor.meshData.normals[bottomNeighbor.topVerts[i]] * 2f);

                    Debug.DrawLine(normalBase, normalTop, Color.green, 2f);
                    Debug.DrawLine(normalBase, neighborTop, Color.red, 2f);


                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.bottomVerts[i]];
                    Vector3 neighborNormal = bottomNeighbor.meshData.normals[bottomNeighbor.topVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.bottomVerts[i]] = vertexNormal;
                    bottomNeighbor.meshData.normals[bottomNeighbor.topVerts[i]] = vertexNormal;

                    Vector3 avgNormal = normalBase + (vertexNormal * 2f);
                    Debug.DrawLine(normalBase, avgNormal, Color.blue, 2f);

                    yield return new WaitForEndOfFrame();
                }

                for (int i = 0; i < chunkResolution; i++) {
                    Destroy(chunkBottomVertices[i]);
                    Destroy(bottomVertices[i]);
                }
            }

            // left
            if (leftNeighbor != null) {
                DemoTextUpdater.current.miscText.text += "Smoothing Left...\n";

                for (int i = 0; i < chunkLength; i++) {
                    GameObject vert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    vert.GetComponent<Renderer>().material.color = Color.green;
                    chunkLeftVertices.Add(vert);
                    vert.transform.position = currChunk.meshData.vertices[currChunk.leftVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y);
                    vert.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize) / mapSize / chunkResolution * 4;
                    
                    // generate neighbor vert
                    GameObject nVert = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    nVert.GetComponent<Renderer>().material.color = Color.red;
                    yield return new WaitForEndOfFrame();
                    leftVertices.Add(nVert);
                    nVert.transform.position = leftNeighbor.meshData.vertices[leftNeighbor.rightVerts[i]] + new Vector3(leftNeighbor.center.x, 5f, leftNeighbor.center.y);
                    nVert.transform.localScale = new Vector3(chunkSize, chunkSize, chunkSize) / mapSize / chunkResolution * 4;

                    float yValue = currChunk.meshData.vertices[currChunk.leftVerts[i]].y;
                    yValue += leftNeighbor.meshData.vertices[leftNeighbor.rightVerts[i]].y;

                    leftNeighbor.meshData.vertices[leftNeighbor.rightVerts[i]].y = yValue / 2f;
                    currChunk.meshData.vertices[currChunk.leftVerts[i]].y = yValue / 2f;
                    yield return new WaitForEndOfFrame();
                    // average value
                    Vector3 avg = vert.transform.position;
                    avg.y = yValue / 2f;

                    vert.transform.position = avg;

                    // normal
                    Vector3 normalBase = currChunk.meshData.vertices[currChunk.leftVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y); ;
                    Vector3 normalTop = normalBase + (currChunk.meshData.normals[currChunk.leftVerts[i]] * 2f);
                    Vector3 neighborTop = normalBase + (leftNeighbor.meshData.normals[leftNeighbor.rightVerts[i]] * 2f);

                    Debug.DrawLine(normalBase, normalTop, Color.green, 2f);
                    Debug.DrawLine(normalBase, neighborTop, Color.red, 2f);


                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.leftVerts[i]];
                    Vector3 neighborNormal = leftNeighbor.meshData.normals[leftNeighbor.rightVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.leftVerts[i]] = vertexNormal;
                    leftNeighbor.meshData.normals[leftNeighbor.rightVerts[i]] = vertexNormal;

                    Vector3 avgNormal = normalBase + (vertexNormal * 2f);
                    Debug.DrawLine(normalBase, avgNormal, Color.blue, 2f);

                    yield return new WaitForEndOfFrame();
                }

                for (int i = 0; i < chunkResolution; i++) {
                    Destroy(chunkLeftVertices[i]);
                    Destroy(leftVertices[i]);
                }
            }

            demoChunks[chunk].GetComponent<Renderer>().material.color = Color.white;

            yield return new WaitForSeconds(timeStep);
            row++;

        }
        cam.orthographic = true;
        cam.transform.position = defaultCamPos;
        cam.orthographicSize = 10f;


        
        // 4. generate mesh
        StartCoroutine(GenerateMeshC());
    }

    IEnumerator SmoothNormalsC() {
        DemoTextUpdater.current.Progress("Smoothing Seam Normals...");
        

        int row = 0;
        int col = 0;
        int chunkLength = chunkResolution;

        cam.orthographic = false;
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(80f, 0f, 0f);
        cam.transform.rotation = rotation;
        //cam.orthographicSize = 0.5f;

        for (int chunk = 0; chunk < numberOfChunks * numberOfChunks; chunk++) {
            // check and reset row
            if (row == numberOfChunks) {
                row = 0;
                col++;
            }

            MapChunk currChunk = mapChunks[row, col];
            MapChunk topNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Top];
            MapChunk rightNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Right];
            MapChunk bottomNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Bottom];
            MapChunk leftNeighbor = currChunk.chunkNeighbors[(int)MapChunkNeighbor.Left];

            cam.transform.position = new Vector3(currChunk.center.x, 8f, currChunk.center.y - 0.5f);

            // top
            for (int i = 0; i < chunkLength; i++) {
                // top
                if (topNeighbor != null) {
                    DemoTextUpdater.current.currentChunk.text = "Chunk( " + row + ", " + col + ")...\n";
                    DemoTextUpdater.current.miscText.text = "Setting Up...\n";
                    DemoTextUpdater.current.miscText.text += "Top Neighbor...\n";

                    Vector3 normalBase = currChunk.meshData.normals[currChunk.topVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y); ;
                    Vector3 normalTop = normalBase + (currChunk.meshData.normals[currChunk.topVerts[i]] * 2f);
                    Vector3 neighborTop = normalBase + (topNeighbor.meshData.normals[topNeighbor.bottomVerts[i]] * 2f);

                    Debug.DrawLine(normalBase, normalTop, Color.green, 100f);
                    Debug.DrawLine(normalBase, neighborTop, Color.red, 100f);


                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.topVerts[i]];
                    Vector3 neighborNormal = topNeighbor.meshData.normals[topNeighbor.bottomVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.topVerts[i]] = vertexNormal;
                    topNeighbor.meshData.normals[topNeighbor.bottomVerts[i]] = vertexNormal;

                    Vector3 avgNormal = normalBase + (vertexNormal * 2f);
                    Debug.DrawLine(normalBase, avgNormal, Color.blue, 100f);
                }

                // right
                if (rightNeighbor != null) {
                    DemoTextUpdater.current.miscText.text += "Right Neighbor...\n";

                    Vector3 normalBase = currChunk.meshData.vertices[currChunk.rightVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y); ;
                    Vector3 normalTop = normalBase + (currChunk.meshData.normals[currChunk.rightVerts[i]] * 2f);
                    Vector3 neighborTop = normalBase + (rightNeighbor.meshData.normals[rightNeighbor.leftVerts[i]] * 2f);

                    Debug.DrawLine(normalBase, normalTop, Color.green, 100f);
                    Debug.DrawLine(normalBase, neighborTop, Color.red, 100f);


                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.rightVerts[i]];
                    Vector3 neighborNormal = rightNeighbor.meshData.normals[rightNeighbor.leftVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.rightVerts[i]] = vertexNormal;
                    rightNeighbor.meshData.normals[rightNeighbor.leftVerts[i]] = vertexNormal;

                    Vector3 avgNormal = normalBase + (vertexNormal * 2f);
                    Debug.DrawLine(normalBase, avgNormal, Color.blue, 100f);
                }

                // bottom
                if (bottomNeighbor != null) {
                    DemoTextUpdater.current.miscText.text += "Bottom Neighbor...\n";

                    Vector3 normalBase = currChunk.meshData.vertices[currChunk.bottomVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y); ;
                    Vector3 normalTop = normalBase + (currChunk.meshData.normals[currChunk.bottomVerts[i]] * 2f);
                    Vector3 neighborTop = normalBase + (bottomNeighbor.meshData.normals[bottomNeighbor.topVerts[i]] * 2f);

                    Debug.DrawLine(normalBase, normalTop, Color.green, 100f);
                    Debug.DrawLine(normalBase, neighborTop, Color.red, 100f);


                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.bottomVerts[i]];
                    Vector3 neighborNormal = bottomNeighbor.meshData.normals[bottomNeighbor.topVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.bottomVerts[i]] = vertexNormal;
                    bottomNeighbor.meshData.normals[bottomNeighbor.topVerts[i]] = vertexNormal;

                    Vector3 avgNormal = normalBase + (vertexNormal * 2f);
                    Debug.DrawLine(normalBase, avgNormal, Color.blue, 100f);
                }

                // left
                if (leftNeighbor != null) {
                    DemoTextUpdater.current.miscText.text += "Left Neighbor...\n";

                    Vector3 normalBase = currChunk.meshData.vertices[currChunk.leftVerts[i]] + new Vector3(currChunk.center.x, 5f, currChunk.center.y); ;
                    Vector3 normalTop = normalBase + (currChunk.meshData.normals[currChunk.leftVerts[i]] * 2f);
                    Vector3 neighborTop = normalBase + (leftNeighbor.meshData.normals[leftNeighbor.rightVerts[i]] * 2f);

                    Debug.DrawLine(normalBase, normalTop, Color.green, 100f);
                    Debug.DrawLine(normalBase, neighborTop, Color.red, 100f);


                    // average normals
                    Vector3 currNormal = currChunk.meshData.normals[currChunk.leftVerts[i]];
                    Vector3 neighborNormal = leftNeighbor.meshData.normals[leftNeighbor.rightVerts[i]];
                    Vector3 vertexNormal = (currNormal + neighborNormal).normalized;

                    currChunk.meshData.normals[currChunk.leftVerts[i]] = vertexNormal;
                    leftNeighbor.meshData.normals[leftNeighbor.rightVerts[i]] = vertexNormal;

                    Vector3 avgNormal = normalBase + (vertexNormal * 2f);
                    Debug.DrawLine(normalBase, avgNormal, Color.blue, 100f);
                }

                yield return new WaitForSeconds(timeStep);
            }

            row++;
        }

        cam.orthographic = true;
        cam.transform.position = defaultCamPos;
        cam.orthographicSize = 10f;

        StartCoroutine(GenerateMeshC());
    }

    IEnumerator GenerateMeshC() {
        DemoTextUpdater.current.Progress("Generating MeshData...");
        speed = 0.6f;

        // 4. generate mesh
        for (int z = 0; z < numberOfChunks; z++) {
            for (int x = 0; x < numberOfChunks; x++) {
                DemoTextUpdater.current.miscText.text = "Creating Mesh...\n";

                // create chunk mesh
                Mesh mesh = mapChunks[x, z].meshData.CreateMesh();

                // create game object of chunk
                GameObject chunk = new GameObject();
                chunk.transform.parent = transform;
                chunk.transform.localScale = Vector3.one;
                chunk.tag = "Chunk";
                chunk.name = "Chunk" + z + ", " + x;

                DemoTextUpdater.current.Chunk(chunk.name);
                DemoTextUpdater.current.miscText.text += "Updating Position...\n";
                chunk.transform.position = new Vector3(mapChunks[x, z].center.x, transform.position.y + 5f, mapChunks[x, z].center.y);
                yield return new WaitForEndOfFrame();
                DemoTextUpdater.current.miscText.text += "Assigning Components...\n";

                chunk.AddComponent<MeshFilter>().sharedMesh = mesh;
                chunk.AddComponent<MeshCollider>().sharedMesh = mesh;
                chunk.AddComponent<MeshRenderer>().sharedMaterial = Instantiate(material);
                chunk.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = mapChunks[x, z].heightmap;

                yield return new WaitForSeconds(timeStep * 2f);
            }
        }

        DemoTextUpdater.current.miscText.text = "";
        
    }

}
