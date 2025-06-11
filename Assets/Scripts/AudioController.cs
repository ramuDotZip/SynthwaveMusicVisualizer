using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource mainAudioSource;
    [SerializeField] private AudioSource secondaryAudioSource;
    [SerializeField] float amplitude = 1f;
    [SerializeField] float globalAmplitude = 1f;
    [SerializeField] float mix = 0.5f;
    [SerializeField] CSGFloatBuffer spectrumDataBuffer;
    [SerializeField] OverrideLabelReader labelReader;

    private const int bufferSize = 9;

    public readonly float[] samplesMain = new float[2048];
    public readonly float[] samplesSecondary = new float[2048];
    public readonly float[] bandVolumes = new float[bufferSize];

    public void StartAnimation() {
        if (mainAudioSource != null) mainAudioSource.Play();
        if (secondaryAudioSource != null) secondaryAudioSource.Play();

        for (int i = 0; i < bufferSize; i++) {
            bandVolumes[i] = 0;
        }

        if (labelReader != null) labelReader.StartAnimation();
    }

    public void ResetAnimation() {
        if (mainAudioSource != null) mainAudioSource.Stop();
        if (secondaryAudioSource != null) secondaryAudioSource.Stop();

        for (int i = 0; i < bufferSize; i++) {
            bandVolumes[i] = 0;
        }

        if (labelReader != null) labelReader.ResetAnimation();
    }

    void Update()
    {
        if (mainAudioSource == null) return;

        mainAudioSource.GetSpectrumData(samplesMain, 0, FFTWindow.Hanning);
        if (secondaryAudioSource != null) secondaryAudioSource.GetSpectrumData(samplesSecondary, 0, FFTWindow.Hanning);

        float[] oldVols = new float[bufferSize];
        for (int i = 0; i < bufferSize; i++) {
            oldVols[i] = bandVolumes[i];
            bandVolumes[i] = 0;
        }

        for (int i = 0; i < 64; i++)
        {
            int position = i * (2 + i / 8);

            float value = samplesMain[position];
            value *= (i + 1) * (i / 64 + 1) * amplitude;

            float mixValue = samplesMain[position] * (1 - mix) + ((secondaryAudioSource != null) ? samplesSecondary[position] : 0) * mix;
            mixValue *= (i + 1) * (i / 64 + 1) * amplitude;

            bandVolumes[8] += mixValue * globalAmplitude;
            bandVolumes[i / 8] += value;
        }

        /*for (int i = 0; i < 512; i++)
        {
            int position = (int)(i * Mathf.Max(i * i / 512f * i / 512f, 64f) / 512f);
            float value = samplesMain[position];
            value *= position;
            //value *= Mathf.Clamp((512 - i) / 64, 0.25f, 1f);
            if (i / 64 == 0 || i / 64 == bufferSize - 1) value *= 0.25f;
            bandVolumes[i / 64] += value / 64;
        }*/
        int sampleRate = AudioSettings.outputSampleRate;

        // Logarithmic, high resolution
        /*for (int i = 0; i < 512; i++)
        {
            int position = (int)Mathf.Pow(2, i * 7f / 512) * 4;
            bandVolumes[i] = samplesMain[position] * (position + 4);
        }*/

        // Logarithmic, frequency accurate, high resolution
        /*for (int i = 0; i < 512; i++)
        {
            // Writing to output at i. Desired frequency:
            float frequency = Mathf.Pow(2, i / 64f + 2) * 16.35f;
            int inputPosition = (int)(frequency / ((float)sampleRate / samplesMain.Length));
            if (inputPosition > samplesMain.Length) continue;
            bandVolumes[i] = samplesMain[inputPosition] * i/8;
        }*/

        // Logarithmic, frequency accurate, low resolution
        /*for (int i = 0; i < 512; i++)
        {
            // Writing to output at i / 64. Desired frequency:
            float frequency = Mathf.Pow(2, i / 64f / 1.5f + 2.5f) * 16.35f;
            int inputPosition = (int)(frequency / ((float)sampleRate / samplesMain.Length));
            if (inputPosition > samplesMain.Length) continue;
            bandVolumes[i / 64] += samplesMain[inputPosition] / 64 * i / 8;
        }*/

        /*for (int i = 0; i < 8; i++)
        {
            bandVolumes[8] += bandVolumes[i] / 8;
        }*/
        

        bandVolumes[8] /= 8;

        /*if (labelReader != null)
        {
            float lrv = labelReader.GetCurrentValue();
            bandVolumes[8] *= Mathf.Max(1 + lrv, 0.5f);
            bandVolumes[8] += lrv * 0.8f + 0.3f;
        }*/

        for (int i = 0; i < bufferSize; i++) {
            float newVol = 1 - Mathf.Exp(-bandVolumes[i]);
            float oldVol = oldVols[i];

            bandVolumes[i] = oldVol + Mathf.Clamp(newVol - oldVol, -(i == 8 ? 0.25f : 1.5f) * Time.deltaTime, 8 * (i == 8 ? 8 : 32) * Time.deltaTime);

            //bandVolumes[i] = Mathf.Lerp(newVol, oldVol, Mathf.Exp(-6 * Time.deltaTime)) + Mathf.Clamp(newVol - oldVol, 0, 0.25f * Time.deltaTime);
            //bandVolumes[i] = Mathf.Lerp(newVol, oldVol, Mathf.Exp(-15 * Time.deltaTime));
        }

        spectrumDataBuffer.Buffer.SetData(bandVolumes);
    }
}
