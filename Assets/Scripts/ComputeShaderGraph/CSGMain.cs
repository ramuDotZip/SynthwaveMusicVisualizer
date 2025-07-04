using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class CSGMain : MonoBehaviour
{
    [SerializeField] private int width = 1920;
    [SerializeField] private int height = 1080;
    [SerializeField] CSGNode finalNode;
    [SerializeField] OverrideLabelReader speedLabelReader;

    private float time = 0;
    private bool playing = false;

    public void StartAnimation()
    {
        time = 0;
        playing = true;

        if (speedLabelReader != null) speedLabelReader.StartAnimation();
    }

    public void ResetAnimation()
    {
        time = 0;
        playing = false;

        if (speedLabelReader != null) speedLabelReader.ResetAnimation();
    }

    private void Start()
    {
        CSGContext context = new()
        {
            width = width,
            height = height
        };
        finalNode.Initialize(context);
    }

    void Update()
    {
        if (playing)
        {
            float elapsed = Time.deltaTime;
            if (speedLabelReader != null)
            {
                elapsed *= speedLabelReader.GetCurrentValue() + 1;
            }

            time += elapsed;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Ceiling division - if the size is not a multiple of 32, round up
        int threadGroupsX = Mathf.CeilToInt(width / 32f);
        int threadGroupsY = Mathf.CeilToInt(height / 32f);

        CSGContext context = new()
        {
            width = width,
            height = height,
            time = time,
            threadGroupsX = threadGroupsX,
            threadGroupsY = threadGroupsY,
            srpRenderImage = source
        };

        finalNode.Dispatch(context);

        Graphics.Blit(finalNode.OutputTexture, destination);
    }
}
