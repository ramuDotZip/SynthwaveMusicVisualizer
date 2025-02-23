using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSGFloatBuffer : MonoBehaviour
{
    [SerializeField] int count;

    public ComputeBuffer Buffer { get; private set; }

    private void OnEnable()
    {
        Buffer = new ComputeBuffer(count, sizeof(float));
    }

    private void OnDisable()
    {
        Buffer.Release();
        Buffer = null;
    }
}
