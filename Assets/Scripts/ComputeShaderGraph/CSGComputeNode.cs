using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSGComputeNode : CSGNode
{
    [SerializeField] ComputeShader shader;
    private ComputeShader shaderInstance;

    [SerializeField] CSGTextureInput[] textureInputs;
    [SerializeField] CSGFloatBufferInput[] floatBufferInputs;

    private bool initialized = false;

    public override void Initialize(CSGContext context)
    {
        if (initialized) return;
        initialized = true;

        foreach (CSGTextureInput textureInput in textureInputs)
        {
            if (textureInput.node != null)
            {
                textureInput.node.Initialize(context);
            }
        }

        OutputTexture = new RenderTexture(context.width, context.height, 0)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            enableRandomWrite = true
        };

        shaderInstance = Instantiate(shader);

        shaderInstance.SetInt("width", context.width);
        shaderInstance.SetInt("height", context.height);
        shaderInstance.SetTexture(0, "output", OutputTexture);

        foreach (CSGTextureInput textureInput in textureInputs)
        {
            if (textureInput.node != null)
            {
                shaderInstance.SetTexture(0, textureInput.inputTextureName, textureInput.node.OutputTexture);
            }
        }

        foreach (CSGFloatBufferInput floatBufferInput in floatBufferInputs)
        {
            if (floatBufferInput.buffer != null)
            {
                shaderInstance.SetBuffer(0, floatBufferInput.inputBufferName, floatBufferInput.buffer.Buffer);
            }
        }
    }

    public override void Dispatch(CSGContext context)
    {
        foreach (CSGTextureInput textureInput in textureInputs)
        {
            if (textureInput.node != null)
            {
                textureInput.node.Dispatch(context);
            }
        }

        shaderInstance.SetFloat("iTime", context.time);
        shaderInstance.Dispatch(0, context.threadGroupsX, context.threadGroupsY, 1);
    }
}
