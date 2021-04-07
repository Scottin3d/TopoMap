using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum NormalizeMode{ 
    Local,
    Global
}

public static class Noise {
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode) {
        float maxPossibleHeight = 0f;
        float amplitude = 1f;
        float frequency = 1f;

        float[,] noiseMap = new float[mapWidth, mapHeight];
        scale = (scale <= 0) ? 0.003f : scale;

        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++) {
            float offsetX = rand.Next(-100000, 100000) + offset.x;
            float offsetZ = rand.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        // for normalization
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int z = 0; z < mapHeight; z++) {
            for (int x = 0; x < mapWidth; x++) {
                amplitude = 1f;
                frequency = 1f;
                float noiseHeight = 0f;


                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleZ = (z - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                maxLocalNoiseHeight = (maxLocalNoiseHeight < noiseHeight) ? noiseHeight : maxLocalNoiseHeight;
                minLocalNoiseHeight = (minLocalNoiseHeight > noiseHeight) ? noiseHeight : minLocalNoiseHeight;

                noiseMap[x, z] = noiseHeight;

            }
        }

        for (int z = 0; z < mapHeight; z++) {
            for (int x = 0; x < mapWidth; x++) {
                if (normalizeMode == NormalizeMode.Local) {
                    noiseMap[x, z] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, z]);
                } else {
                    float normalizedHeight = (noiseMap[x, z] + 1) / (2f * maxPossibleHeight / 1.3f);
                    noiseMap[x, z] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }

    public static float[,] GenerateNoiseMapFromHeightmap(Texture2D heightmap) {
        int width = heightmap.width;
        int height = heightmap.height;
        float[,] noiseMap = new float[width, height];

        Color[] pixelColors = new Color[width * height];
        pixelColors = heightmap.GetPixels();
        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                noiseMap[x, z] = pixelColors[z * width + x].grayscale;
            }
        }
        return noiseMap;
    }

    public static float[,] GenerateNoiseMapFromHeightmapWithCurve(Texture2D heightmap, AnimationCurve riverCurve) {
        int width = heightmap.width;
        int height = heightmap.height;
        float[,] noiseMap = new float[width, height];

        int riverTimeIncrement = 1 / width;

        float[,] riverMap = new float[width, height];
        Vector2[] rMap = new Vector2[width];
        for (float i = 0; i < width; i++) {
            rMap[(int)i] = new Vector2(i, riverCurve.Evaluate(i / width));
        }

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {

                int curveValue = Mathf.RoundToInt(riverCurve.Evaluate(x) * height);
                curveValue = (curveValue == z) ? curveValue : 0;
                riverMap[x, z] = curveValue;
            }
        }
        
        Color[] pixelColors = new Color[width * height];
        pixelColors = heightmap.GetPixels();

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                noiseMap[x, z] = pixelColors[z * width + x].grayscale;

                if (Mathf.RoundToInt(rMap[x].y * height) >= z - 5 && Mathf.RoundToInt(rMap[x].y * height) <= z + 5) {
                    noiseMap[x, z] *= 0;
                }
            }
        }
        return noiseMap;
    }
}
