using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour {
    [SerializeField] private int width = 1920;
    [SerializeField] private int height = 1080;
    private RenderTexture texFP, texFXAA, texGX, texGY, texGX2, texGY2, texBand, texFM, texCI;
    [SerializeField] private ComputeShader firstPassShader, gaussianXShader, gaussianYShader, finalMixShader, fxaaShader, visualizerBandShader;
    private ComputeShader gaussianX2Shader, gaussianY2Shader;
    private ComputeBuffer spectrumDataBuffer;
    //[SerializeField] Texture2D centerImage;
    private AudioController audioController;
    private float time = 0;
    private bool playing = false;

    private void OnEnable() {
        spectrumDataBuffer = new ComputeBuffer(9, sizeof(float));
    }

    private void OnDisable() {
        spectrumDataBuffer.Release();
        spectrumDataBuffer = null;
    }

    public void StartAnimation() {
        time = 0;
        playing = true;
    }

    public void ResetAnimation() {
        time = 0;
        playing = false;
    }

    // Start is called before the first frame update
    void Start() {
        audioController = GetComponent<AudioController>();

        gaussianX2Shader = Instantiate(gaussianXShader);
        gaussianY2Shader = Instantiate(gaussianYShader);

        InitializeRenderTexture(out texFP);
        InitializeRenderTexture(out texFXAA);
        InitializeRenderTexture(out texGX);
        InitializeRenderTexture(out texGY);
        InitializeRenderTexture(out texGX2);
        InitializeRenderTexture(out texGY2);
        InitializeRenderTexture(out texBand);
        InitializeRenderTexture(out texFM);
        InitializeRenderTexture(out texCI);

        InitializeComputeShader(firstPassShader, texFP);
        InitializeComputeShader(fxaaShader, texFXAA);
        InitializeComputeShader(gaussianXShader, texGX);
        InitializeComputeShader(gaussianYShader, texGY);
        InitializeComputeShader(gaussianX2Shader, texGX2);
        InitializeComputeShader(gaussianY2Shader, texGY2);
        InitializeComputeShader(visualizerBandShader, texBand);
        InitializeComputeShader(finalMixShader, texFM);
        

        fxaaShader.SetTexture(0, "input", texFP);
        gaussianXShader.SetTexture(0, "input", texFXAA);
        gaussianYShader.SetTexture(0, "input", texGX);
        visualizerBandShader.SetBuffer(0, "spectrumData", spectrumDataBuffer);
        gaussianX2Shader.SetTexture(0, "input", texBand);
        gaussianY2Shader.SetTexture(0, "input", texGX2);
        finalMixShader.SetTexture(0, "firstPassInput", texFXAA);
        finalMixShader.SetTexture(0, "gaussianInput", texGY);
        finalMixShader.SetTexture(0, "bandsInput", texBand);
        finalMixShader.SetTexture(0, "bandsBlurredInput", texGY2);
        finalMixShader.SetTexture(0, "centerImageInput", texCI);
        finalMixShader.SetBuffer(0, "spectrumData", spectrumDataBuffer);
    }

    void InitializeRenderTexture(out RenderTexture texture) {
        texture = new RenderTexture(width, height, 0);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.enableRandomWrite = true;
    }

    void InitializeComputeShader(ComputeShader shader, RenderTexture outputTexture) {
        shader.SetInt("width", width);
        shader.SetInt("height", height);
        shader.SetTexture(0, "output", outputTexture);
    }

    void UpdateShaderVars(ComputeShader shader) {
        shader.SetFloat("iTime", time);
    }

    // Update is called once per frame
    void Update()
    {
        if (playing) time += Time.deltaTime;

        if (audioController != null) {
            spectrumDataBuffer.SetData(audioController.bandVolumes);
        }

        UpdateShaderVars(firstPassShader);
        UpdateShaderVars(finalMixShader);

        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {

        // Ceiling division - if the size is not a multiple of 32, round up
        int threadGroupsX = Mathf.CeilToInt(width / 32f);
        int threadGroupsY = Mathf.CeilToInt(height / 32f);

        // Execute all shaders
        firstPassShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        fxaaShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        gaussianXShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        gaussianYShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        visualizerBandShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        gaussianX2Shader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        gaussianY2Shader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.SetRenderTarget(texCI, 0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1920, 1080, 0);
        GL.Clear(true, true, new Color(0,0,0,0));
        Graphics.DrawTexture(new Rect(0, 0, width, height), source);
        GL.PopMatrix();

        finalMixShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(texFM, destination);

        /*Graphics.SetRenderTarget(destination, 0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1920, 1080, 0);
        Graphics.Blit(texFM, destination);
        //Graphics.DrawTexture(new Rect(0, 0, width, height), source, new Rect(0, 0, width, height), 0, 0, 0, 0);
        Graphics.DrawTexture(new Rect(0, 0, width, height), source);
        GL.PopMatrix();*/
    }
}
