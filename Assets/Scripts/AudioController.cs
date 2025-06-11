using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource mainAudioSource;
    [SerializeField] private AudioSource secondaryAudioSource;
    [SerializeField] float startTime = 0;
    [SerializeField] float modAmplitude = 1f;
    [SerializeField] float worldModAmplitude = 1f;
    [SerializeField] float worldModCompressionGain = 2f;
    [SerializeField] float worldModCompressionRatio = 2f;
    [SerializeField] float worldModCompressionAttack = 0.2f;
    [SerializeField] float mix = 0.5f;
    [SerializeField] CSGFloatBuffer spectrumDataBuffer;
    [SerializeField] OverrideLabelReader powerLabelReader;
    [SerializeField] OverrideLabelReader speedLabelReader;

    private const int bufferSize = 10;

    public readonly float[] samplesMain = new float[2048];
    public readonly float[] samplesSecondary = new float[2048];
    public readonly float[] bandVolumes = new float[bufferSize];

    private float previousWM = 0;
    private float worldModCompressionReduction = 0;

    public void StartAnimation() {
        if (mainAudioSource != null) PlayAudioAtTime(mainAudioSource, startTime);
        if (secondaryAudioSource != null) PlayAudioAtTime(secondaryAudioSource, startTime);

        for (int i = 0; i < bufferSize; i++) {
            bandVolumes[i] = 0;
        }

        if (powerLabelReader != null) powerLabelReader.StartAnimation();
        if (speedLabelReader != null) speedLabelReader.StartAnimation();
    }

    private void PlayAudioAtTime(AudioSource source, float startTime)
    {
        if (startTime >= 0)
        {
            source.time = startTime;
            source.Play();
        }
        else
        {
            source.PlayDelayed(-startTime);
        }
    }

    public void ResetAnimation() {
        if (mainAudioSource != null) mainAudioSource.Stop();
        if (secondaryAudioSource != null) secondaryAudioSource.Stop();

        for (int i = 0; i < bufferSize; i++) {
            bandVolumes[i] = 0;
        }

        if (powerLabelReader != null) powerLabelReader.ResetAnimation();
        if (speedLabelReader != null) speedLabelReader.ResetAnimation();
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
            value *= (i + 1) * (i / 64 + 1) * modAmplitude;

            float mixValue = samplesMain[position] * (1 - mix) + ((secondaryAudioSource != null) ? samplesSecondary[position] : 0) * mix;
            mixValue *= (i + 1) * (i / 64 + 1) * modAmplitude;

            bandVolumes[8] += mixValue * (i / 32) * (i / 32) * worldModAmplitude;
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

        if (powerLabelReader != null)
        {
            float lrv = powerLabelReader.GetCurrentValue();
            bandVolumes[8] *= Mathf.Max(1 + lrv, 0.5f);
            bandVolumes[8] += lrv * 0.8f + 0.3f;
        }

        for (int i = 0; i < 8; i++) {
            float newVol = 1 - Mathf.Exp(-bandVolumes[i]);
            float oldVol = oldVols[i];

            bandVolumes[i] = oldVol + Mathf.Clamp((1 - Mathf.Exp(-15 * Time.deltaTime * (newVol + 1))) * (newVol - oldVol), -(1.5f) * Time.deltaTime, 8 * (32) * Time.deltaTime);

            //bandVolumes[i] = Mathf.Lerp(newVol, oldVol, Mathf.Exp(-6 * Time.deltaTime)) + Mathf.Clamp(newVol - oldVol, 0, 0.25f * Time.deltaTime);
            //bandVolumes[i] = Mathf.Lerp(newVol, oldVol, Mathf.Exp(-15 * Time.deltaTime));
        }

        float newWM = (1 - Mathf.Exp(-bandVolumes[8])) * worldModCompressionGain;
        float oldWM = previousWM; //oldVols[8];
        float finalWM = Mathf.Lerp(newWM, oldWM, Mathf.Exp(-1 * Time.deltaTime)) + Mathf.Clamp(newWM - oldWM - 0.05f * worldModCompressionGain, 0, 8f * worldModCompressionGain * Time.deltaTime);

        
        worldModCompressionReduction = Mathf.Lerp(finalWM * (1 - (1 / worldModCompressionRatio)), worldModCompressionReduction, Mathf.Exp(-2 * Time.deltaTime / worldModCompressionAttack));
        //Debug.Log("New WM: " + newWM + ", Final WM before compression: " + finalWM + ", Reduction: " + worldModCompressionReduction + ", Final WM after compression: " + (finalWM - worldModCompressionReduction));
        previousWM = finalWM;
        finalWM -= worldModCompressionReduction;
        bandVolumes[8] = finalWM;

        bandVolumes[9] = (speedLabelReader != null) ? speedLabelReader.GetCurrentValue() : 0;

        spectrumDataBuffer.Buffer.SetData(bandVolumes);
    }
}
