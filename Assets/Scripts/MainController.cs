using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    AudioController audioController;
    CSGMain shaderController;
    // Start is called before the first frame update
    void Start()
    {
        audioController = GetComponent<AudioController>();
        shaderController = GetComponent<CSGMain>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) {
            if (audioController != null) audioController.StartAnimation();
            if (shaderController != null) shaderController.StartAnimation();
        }
        if (Input.GetKeyDown(KeyCode.F2)) {
            if (audioController != null) audioController.ResetAnimation();
            if (shaderController != null) shaderController.ResetAnimation();
        }
    }
}
