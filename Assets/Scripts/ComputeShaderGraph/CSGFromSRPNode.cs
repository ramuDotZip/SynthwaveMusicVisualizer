using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSGFromSRPNode : CSGNode
{
    public override void Initialize(CSGContext context)
    {
        OutputTexture = new RenderTexture(context.width, context.height, 0);
    }

    public override void Dispatch(CSGContext context)
    {
        Graphics.Blit(context.srpRenderImage, OutputTexture);
    }
}
