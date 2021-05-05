using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public static class MeshGenerator {
    /// <summary>
    /// GenerateTerrainMesh:Generates a mesh using a float[,] of height values.
    /// Values are normalized.
    /// </summary>
    /// <param name="chunk"> The chunk data for the mesh.</param>
    /// <param name="heightMulitplier"> A multiplier to scale the height of the mesh.</param>
    /// <param name="meshHieghtCurve">A curve to sample the height values from.</param>
    /// <param name="chunkSize">The size of the chunk in world space units.</param>
    /// <param name="levelOfDetail">The level of detail the chunk is going to be created at.</param>
    /// <returns>The generated meshData of the chunk.</returns>
    public static MeshData GenerateTerrainMesh(MapChunk chunk, float heightMulitplier, AnimationCurve meshHieghtCurve, float chunkSize, int levelOfDetail) {
        AnimationCurve heightCurve = new AnimationCurve(meshHieghtCurve.keys);                  // create a local copy of the curve.  There were issues when using threading about sampling the correct value
        int width = chunk.mapData.heightValues.GetLength(0);
        int height = chunk.mapData.heightValues.GetLength(1);

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;         // this step determines the resolution of the mesh based on the level of detail
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        float meshStep = chunkSize / (width - 1);
        float lowerLeftX = chunkSize / -2f;                                                     // meshes, like chunks, are create from the lower left corner, left -> right, bottom -> top order
        float lowerLeftZ = chunkSize / -2f;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);                     // create the container for the generated data
        
        int vertexIndex = 0;
        for (int z = 0; z < height; z += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                // calculate the center of the quad
                Vector2 quadCenter = new Vector2(lowerLeftX + (x * meshStep), lowerLeftZ + (z * meshStep));

                // calculate the Vector3
                float xVal = quadCenter.x;
                float yVal = heightCurve.Evaluate(chunk.mapData.heightValues[x, z]) * heightMulitplier;
                float zVal = quadCenter.y;

                // store the vertex and uv data in the container
                meshData.vertices[vertexIndex] = new Vector3(xVal, yVal, zVal);
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, z / (float)height);

                // find world min and max values
                if (yVal > GenerateMapFromHeightMap.worldMaxHeight) {
                    GenerateMapFromHeightMap.worldMaxHeight = yVal;
                }

                if (yVal < GenerateMapFromHeightMap.worldMinHeight) {
                    GenerateMapFromHeightMap.worldMinHeight = yVal;
                }

                // check and store edge vertices for later use in smoothing the mesh seams
                // top
                if (z == height - 1) {
                    chunk.topVerts.Add(vertexIndex);
                    // top left
                    if (x == 0) {
                        chunk.cornerVerts.Add(vertexIndex);
                    }
                    // top right
                    if (x == width - 1) {
                        chunk.cornerVerts.Add(vertexIndex);
                    }
                }

                // right
                if (x == width - 1) {
                    chunk.rightVerts.Add(vertexIndex);
                }

                // bottom
                if (z == 0) {
                    chunk.bottomVerts.Add(vertexIndex);
                    // bottom left
                    if (x == 0) {
                        chunk.cornerVerts.Add(vertexIndex);
                    }
                    // bottom right
                    if (x == width - 1) {
                        chunk.cornerVerts.Add(vertexIndex);
                    }
                }

                // left side
                if (x == 0) {
                    chunk.leftVerts.Add(vertexIndex);
                }
                
                if (x < width - 1 && z < height - 1) {
                    // in Unity, the culling direction is clockwise
                    // depending on how the mesh is constructed (L/R, T/B) the order of the 
                    // vertices in the triangle array will be different

                    // top to bottom
                    /*
                    meshData.AddTriangle(vertexIndex,
                                         vertexIndex + verticesPerLine + 1,
                                         vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1,
                                         vertexIndex,
                                         vertexIndex + 1);
                    */
                    // bottom to top
                    meshData.AddTriangle(vertexIndex,
                                         vertexIndex + verticesPerLine,
                                         vertexIndex + 1);
                    meshData.AddTriangle(vertexIndex + verticesPerLine,
                                         vertexIndex + verticesPerLine + 1,
                                         vertexIndex + 1);
                }
                vertexIndex++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.uv = meshData.uv;
        mesh.RecalculateNormals();
        meshData.normals = mesh.normals;
        return meshData;
    }
}

/// <summary>
/// MeshData: Stores all of the object data for the terrain chunk mesh.
/// </summary>
public class MeshData {
    public Mesh chunkMesh;
    public int chunkSize;
    public Vector3[] vertices;
    public Vector3[] normals;

    int triangleIndex;
    public int[] triangles;
    public Vector2[] uv;

    /// <summary>
    /// MeshData: Constructor for the object.
    /// </summary>
    /// <param name="meshWidth">The width of the mesh.</param>
    /// <param name="meshHeight">The height of the mesh.</param>
    public MeshData(int meshWidth, int meshHeight) {
        chunkSize = meshWidth;
        int size = meshWidth * meshHeight;
        vertices = new Vector3[size];
        normals = new Vector3[size];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 2 * 3];
        uv = new Vector2[size];
    }

    /// <summary>
    /// AddTriangle: Adds two triangles to the mesh triangle array.
    /// </summary>
    /// <param name="a">Vertex A.</param>
    /// <param name="b">Vertex B.</param>
    /// <param name="c">Vertex C.</param>
    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    /// <summary>
    /// CreateMesh: Creates the actual mesh for the object.
    /// </summary>
    /// <returns></returns>
    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.normals = normals;
        chunkMesh = mesh;
        return mesh;
    }

    public void RecalulateNormals() {
        chunkMesh.RecalculateNormals();
    }
}
