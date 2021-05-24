using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public static class Noise {
    /// <summary>
    /// GenerateNoiseMapFromHeightmap: Generates a float[,] from a heightmap.
    /// </summary>
    /// <param name="heightmap">The heightmap used to generate the height information.</param>
    /// <returns>a float[,] of height values.</returns>
    public static float[,] GenerateNoiseMapFromHeightmap(Texture2D heightmap, NoiseProperties noiseProperties) {
        int width = heightmap.width;
        int height = heightmap.height;
        float[,] noiseMap = new float[width, height];
        Color[] pixelColors = new Color[width * height];
        pixelColors = heightmap.GetPixels();

        float maxPossibleHeight = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float scale = noiseProperties.noiseScale;
        scale = (scale <= 0) ? 0.003f : scale;

        System.Random rand = new System.Random(noiseProperties.seed);
        Vector2[] octaveOffsets = new Vector2[noiseProperties.octaves];

        for (int i = 0; i < noiseProperties.octaves; i++) {
            float offsetX = rand.Next(-100000, 100000);
            float offsetZ = rand.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);

            maxPossibleHeight += amplitude;
            amplitude *= noiseProperties.persistence;
        }

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;


        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                amplitude = 1f;
                frequency = 1f;
                float noiseHeight = 0f;
                float pixel = pixelColors[z * width + x].grayscale;

                for (int i = 0; i < noiseProperties.octaves; i++) {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleZ = (z - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f;
                    noiseHeight += ((noiseProperties.noiseInfluence * perlinValue * amplitude) + pixel) /2f;

                    amplitude *= noiseProperties.persistence;
                    frequency *= noiseProperties.lacunarity;
                }


                

                noiseMap[x, z] = pixel;
                //noiseMap[x, z] = noiseHeight;

            }
        }

        return noiseMap;
    }

    /// <summary>
    /// Create a float[,] Perlin Noise map.
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="seed"></param>
    /// <param name="scale"></param>
    /// <param name="octaves"></param>
    /// <param name="persistance"></param>
    /// <param name="lacunarity"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
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

                noiseMap[x, z] = Mathf.Clamp01(noiseHeight);

            }
        }

        return noiseMap;
    }
}
