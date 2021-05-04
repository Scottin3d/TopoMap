using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {

    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] noiseMap) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);

        Color[] pixelColors = new Color[width * height];

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                pixelColors[z * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);
            }
        }

        return TextureFromColorMap(pixelColors, width, height);
    }

    /// <summary>
    /// GetPixelMap: Extracts a portion of a larger texture map.
    /// </summary>
    /// <param name="width">The width of the map to extract.</param>
    /// <param name="height">The height of the map to extract.</param>
    /// <param name="size">The pixel size of the map to return.</param>
    /// <returns></returns>
    public static Texture2D GetPixelMap(Texture2D basemap, int width, int height, int size) {
        Color[] pixelColors = new Color[size * size];
        pixelColors = basemap.GetPixels(width, height, size, size);
        return TextureGenerator.TextureFromColorMap(pixelColors, size, size);
    }

    /// <summary>
    /// Converts a non-power of 2 texture into a power of 2 texture.
    /// </summary>
    /// <param name="basemap"></param>
    /// <returns></returns>
    public static Texture2D ConvertTextureToPowerOf2(Texture2D basemap) {
        int width = basemap.width;
        int height = basemap.height;
        int size = 0;
        // already pow 2
        if (width == height) {
            return basemap;
        }

        if (width > height) {
            size = height;
        } else {
            size = width;
        }

        Color[] pixelColors = new Color[size * size];
        pixelColors = basemap.GetPixels(0, 0, size, size);
        return TextureGenerator.TextureFromColorMap(pixelColors, size, size);

    }
}
