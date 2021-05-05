using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureConvertTest : MonoBehaviour
{

    public Texture2D nonPowerOf2Texture;
    public Color[] pixels;
    // Start is called before the first frame update
    void Start()
    {
        ConvertTexture();
    }

    private void ConvertTexture() {
        Texture2D texture = TextureGenerator.ConvertTextureToPowerOf2(nonPowerOf2Texture);
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.GetComponent<Renderer>().material.mainTexture = texture;

        pixels = new Color[texture.width * texture.height];
        pixels = texture.GetPixels(0, 0, 64, 64);
    }
}
