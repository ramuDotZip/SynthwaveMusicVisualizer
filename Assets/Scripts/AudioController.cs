using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource mainAudioSource;
    [SerializeField] private AudioSource secondaryAudioSource;
    [SerializeField] float mix = 0.5f;
    [SerializeField] CSGFloatBuffer spectrumDataBuffer;
    [SerializeField] OverrideLabelReader labelReader;

    public readonly float[] samplesMain = new float[2048];
    public readonly float[] samplesSecondary = new float[2048];
    public readonly float[] bandVolumes = new float[9];

    public void StartAnimation() {
        if (mainAudioSource != null) mainAudioSource.Play();
        if (secondaryAudioSource != null) secondaryAudioSource.Play();

        for (int i = 0; i < 9; i++) {
            bandVolumes[i] = 0;
        }

        labelReader.StartAnimation();
    }

    public void ResetAnimation() {
        if (mainAudioSource != null) mainAudioSource.Stop();
        if (secondaryAudioSource != null) secondaryAudioSource.Stop();

        for (int i = 0; i < 9; i++) {
            bandVolumes[i] = 0;
        }

        labelReader.ResetAnimation();
    }

    void Update()
    {
        if (mainAudioSource == null || secondaryAudioSource == null) return;

        mainAudioSource.GetSpectrumData(samplesMain, 0, FFTWindow.Hanning);
        secondaryAudioSource.GetSpectrumData(samplesSecondary, 0, FFTWindow.Hanning);

        float[] oldVols = new float[9];
        for (int i = 0; i < 9; i++) {
            oldVols[i] = bandVolumes[i];
            bandVolumes[i] = 0;
        }

        for (int i = 0; i < 64; i++) {
            int position = i * (2 + i / 8);
            float value = samplesMain[position] * (1 - mix) + samplesSecondary[position] * mix;
            value *= (i + 1) * (i / 64 + 1);
            bandVolumes[8] += value;
            bandVolumes[i / 8] += value;
        }

        bandVolumes[8] /= 8;
        float lrv = labelReader.GetCurrentValue();
        bandVolumes[8] *= Mathf.Max(1 + lrv, 0.5f);
        bandVolumes[8] += lrv * 0.8f + 0.3f;
        for (int i = 0; i < 9; i++) {
            float newVol = 1 - Mathf.Exp(-bandVolumes[i]);
            float oldVol = oldVols[i];

            //bandVolumes[i] = oldVol + Mathf.Clamp(newVol - oldVol, -(i == 8 ? 0.5f : 1.5f) * Time.deltaTime, 8 * (i == 8 ? 8 : 32) * Time.deltaTime);

            bandVolumes[i] = Mathf.Lerp(newVol, oldVol, Mathf.Exp(-15 * Time.deltaTime)) + Mathf.Clamp(newVol - oldVol, 0, 4 * Time.deltaTime);
        }

        spectrumDataBuffer.Buffer.SetData(bandVolumes);
    }
}
