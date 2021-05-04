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

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                float normalizedHeight = (noiseMap[x, z] + 1) / (2f * maxPossibleHeight / 1.3f);
                noiseMap[x, z] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            }
        }

        return noiseMap;
    }
}
