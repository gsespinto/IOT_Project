using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkipIntro : MonoBehaviour
{
    [SerializeField] private GameObject pressTip;
    [SerializeField] private Image pressImage;
    [SerializeField] private float pressTime = 0.5f;
    private float currentPressTime;
    private bool isPressing = false;
    private bool hasTriggeredSkip = false;
    private InputComponent _inputComponent;
    
    // Start is called before the first frame update
    void Start()
    {
        _inputComponent = GameObject.FindObjectOfType<InputComponent>();
        _inputComponent.ButtonClick += StartPressing;
        _inputComponent.ButtonUp += StopPressing;
        
        StopPressing();
    }

    private void Update()
    {
        HandlePress();
    }

    void HandlePress()
    {
        if (!isPressing || hasTriggeredSkip)
            return;

        if (currentPressTime <= 0)
        {
            hasTriggeredSkip = true;
            LoadingManager.LoadScene(1);
            return;
        }

        currentPressTime -= Time.deltaTime;
        pressImage.fillAmount = 1 - currentPressTime / pressTime;
    }
    
    void StartPressing()
    {
        Debug.Log("Button down");
        pressTip.SetActive(false);
        pressImage.fillAmount = 1 - currentPressTime / pressTime;
        pressImage.gameObject.SetActive(true);
        isPressing = true;
    }
    
    void StopPressing()
    {
        Debug.Log("Button up");
        pressImage.gameObject.SetActive(false);
        currentPressTime = pressTime;
        isPressing = false;
    }
}
