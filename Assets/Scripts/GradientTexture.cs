using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GradientTexture : MonoBehaviour
{
    [SerializeField] Gradient gradient;
    [SerializeField] int height = 256;
    [SerializeField] string path = "/Gradients/FontGradient.png";
    
    private Texture2D GenerateTexture(Gradient gradient)
    {
        Texture2D tex = new(1, height);

        for (int i = 0; i < height; i++)
        {
            tex.SetPixel(0, i, gradient.Evaluate((float) i / height));
        }

        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();

        return tex;
    }

    public void GenerateAndBakeTexture()
    {
        Texture2D tex = GenerateTexture(gradient);
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + path, bytes);
        Debug.Log("GradientTexture: saved image to file " + path);
    }
}
