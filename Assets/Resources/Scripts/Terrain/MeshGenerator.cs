using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightmap, float heightMulitplier, AnimationCurve meshHieghtCurve, int levelOfDetail) {
        AnimationCurve heightCurve = new AnimationCurve(meshHieghtCurve.keys);
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int z = 0; z < height; z+= meshSimplificationIncrement) {
            for (int x = 0; x < width; x+= meshSimplificationIncrement) {


                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightmap[x, z]) * heightMulitplier, topLeftZ - z);
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, z/ (float)height);

                if (x < width -1 && z < height - 1) {
                    meshData.AddTriangle(vertexIndex, 
                                         vertexIndex + verticesPerLine + 1, 
                                         vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, 
                                         vertexIndex, 
                                         vertexIndex + 1);
                }
                vertexIndex++;
            }
        }

        return meshData;
    }

    public static MeshData GenerateTerrainMesh(float[,] heightmap, float heightMulitplier, AnimationCurve meshHieghtCurve, float chunkSize, int levelOfDetail, Vector2 center) {
        AnimationCurve heightCurve = new AnimationCurve(meshHieghtCurve.keys);
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        float meshStep = chunkSize / (width - 1);

        float lowerLeftX = chunkSize / -2f;
        float lowerLeftZ = chunkSize / -2f;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int z = 0; z < height; z += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {
                
                Vector2 quadCenter = new Vector2(lowerLeftX + (x * meshStep), lowerLeftZ + (z * meshStep));

                //float xVal = topLeftX + (x * meshStep);
                float xVal = quadCenter.x;
                float yVal = heightCurve.Evaluate(heightmap[x, z]) * heightMulitplier;
                //float zVal = topLeftZ - (z * meshStep);
                float zVal = quadCenter.y;

                meshData.vertices[vertexIndex] = new Vector3(xVal, yVal, zVal);
                meshData.uv[vertexIndex] = new Vector2(x / (float)width, z / (float)height);


                
                if (x < width - 1 && z < height - 1) {
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

        return meshData;
    }
}

public class MeshData {
    public int chunkSize;
    public Vector3[] vertices;
    int triangleIndex;
    public int[] triangles;
    public Vector2[] uv;

    // this tutorial uses -1 where the MeshWidth is the number of 
    // vertices and not the number of faces
    public MeshData(int meshWidth, int meshHeight) {
        chunkSize = meshWidth;
        int size = meshWidth * meshHeight;
        vertices = new Vector3[size];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 2 * 3];
        uv = new Vector2[size];
    }

    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        return mesh;
    }
}
