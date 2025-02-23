using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CSGNode : MonoBehaviour
{
    public RenderTexture OutputTexture { get; protected set; }

    public abstract void Initialize(CSGContext context);

    public abstract void Dispatch(CSGContext context);
}
