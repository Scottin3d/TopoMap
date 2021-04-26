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
}
