using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverrideLabelReader : MonoBehaviour
{
    [SerializeField] TextAsset labels;
    [SerializeField] float startTime = 0;
    private string[] lines;
    private int lineNumber;
    private float currentSmoothedValue, currentEnvValue, envStartTime, envEndTime, envStartVal, envEndVal;

    private float time = 0;
    private bool playing = false;

    void Start()
    {
        lines = (labels != null) ? labels.text.Split("\n") : new string[0];
        ResetLineReader();
    }

    
    void Update()
    {
        if (playing) time += Time.deltaTime;

        if (time > envEndTime)
        {
            while (time > envEndTime)
            {
                currentEnvValue = envEndVal;
                ReadLine();
            }
        }
        else if (time > envStartTime)
        {
            currentEnvValue = envStartVal + (envEndVal - envStartVal) * ((time - envStartTime) / (envEndTime - envStartTime));
        }
        else
        {
            currentEnvValue = 0;
        }
        currentSmoothedValue = Mathf.Lerp(currentEnvValue, currentSmoothedValue, Mathf.Exp(-15 * Time.deltaTime));
    }

    public float GetCurrentValue()
    {
        return currentSmoothedValue;
    }

    private void ResetLineReader()
    {
        lineNumber = 0;
        ReadLine();
    }

    private void ReadLine()
    {
        
        string currentLine;
        string[] parts;
        do
        {
            if (lineNumber >= lines.Length)
            {
                envStartTime = float.MaxValue;
                envEndTime = float.MaxValue;
                return;
            }
            currentLine = lines[lineNumber++];
            parts = currentLine.Split(new string[] { "\t", " " }, StringSplitOptions.None);
        } while (parts.Length < 4);
        
        envStartTime = float.Parse(parts[0]);
        envEndTime = float.Parse(parts[1]);
        envStartVal = float.Parse(parts[2]);
        envEndVal = float.Parse(parts[3]);
    }

    public void StartAnimation()
    {
        time = startTime;
        playing = true;
        ResetLineReader();
    }

    public void ResetAnimation()
    {
        time = startTime;
        playing = false;
        ResetLineReader();
    }

}
