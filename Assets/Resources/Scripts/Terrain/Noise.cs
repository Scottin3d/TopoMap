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
    public static float[,] GenerateNoiseMapFromHeightmap(Texture2D heightmap) {
        int width = heightmap.width;
        int height = heightmap.height;
        float[,] noiseMap = new float[width, height];
        
        Color[] pixelColors = new Color[width * height];
        pixelColors = heightmap.GetPixels();
        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                float pixel = pixelColors[z * width + x].grayscale;

                noiseMap[x, z] = pixel;
            }
        }
        
        return noiseMap;
    }
}
