using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class TextController : MonoBehaviour
{
    [SerializeField] Gradient textGradient;
    // Start is called before the first frame update
    void Start()
    {
        

        /*Texture2D tex = new Texture2D(1, 1);

        for (int i = 0; i < 1; i++) {
            //tex.SetPixel(0, i, grad.Evaluate(i / 256.0f));
            tex.SetPixel(0, i, new Color(1, 1, 1, 1));
        }

        Material material = gameObject.GetComponent<TextMeshProUGUI>().fontSharedMaterial;
        //Debug.Log("Material: " + material);
        material.SetTexture("_FaceTex", tex);*/
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
