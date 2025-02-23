using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GradientTexture))]
public class GradientTextureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Gradient Texture"))
        {
            ((GradientTexture)target).GenerateAndBakeTexture();
            AssetDatabase.Refresh();
        }
    }
}
