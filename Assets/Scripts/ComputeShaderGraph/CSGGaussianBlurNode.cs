using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSGGaussianBlurNode : CSGNode
{
    [SerializeField] ComputeShader shaderX;
    [SerializeField] ComputeShader shaderY;
    private ComputeShader shaderInstanceX;
    private ComputeShader shaderInstanceY;

    [SerializeField] CSGNode input;

    private bool initialized = false;

    public override void Initialize(CSGContext context)
    {
        if (initialized) return;
        initialized = true;

        input.Initialize(context);

        OutputTexture = new RenderTexture(context.width, context.height, 0)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            enableRandomWrite = true
        };

        RenderTexture intermediateTexture = new RenderTexture(context.width, context.height, 0)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            enableRandomWrite = true
        };

        shaderInstanceX = Instantiate(shaderX);

        shaderInstanceY = Instantiate(shaderY);

        shaderInstanceX.SetInt("width", context.width);
        shaderInstanceX.SetInt("height", context.height);
        shaderInstanceX.SetTexture(0, "input", input.OutputTexture);
        shaderInstanceX.SetTexture(0, "output", intermediateTexture);

        shaderInstanceY.SetInt("width", context.width);
        shaderInstanceY.SetInt("height", context.height);
        shaderInstanceY.SetTexture(0, "input", intermediateTexture);
        shaderInstanceY.SetTexture(0, "output", OutputTexture);
    }

    public override void Dispatch(CSGContext context)
    {
        input.Dispatch(context);

        shaderInstanceX.SetFloat("iTime", context.time);
        shaderInstanceX.Dispatch(0, context.threadGroupsX, context.threadGroupsY, 1);

        shaderInstanceY.SetFloat("iTime", context.time);
        shaderInstanceY.Dispatch(0, context.threadGroupsX, context.threadGroupsY, 1);
    }
}
