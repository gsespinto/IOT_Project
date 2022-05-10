using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IOT_Delegates;

public class InputComponent : MonoBehaviour
{
  [SerializeField] private bool simulateLightInput = false;
  [SerializeField] private bool simulateFreqInput = false;
  [SerializeField] private bool simulateButtonInput = false;
  [Space(20)]
  [SerializeField] private SerialController ardityController;
  [SerializeField] private float flashLightInputTimer = 0.25f;
  private float _currentLightTimer = 0.0f;
  
  private bool _lightInput = false;
  private float _lastFreqInput = 0;
  private bool _currentButtonInput = false;
  private bool _lastButtonInput = false;

  private bool _hasFiredFlashLightEvent = false;
  private bool _hasFiredMovementEvent = false;

  public BoolDelegate FlashLightEvent;
  public BoolDelegate MovementEvent;
  public NoParamsDelegate PhotoEvent;
  public IntDelegate FrequencyEvent; 
  public NoParamsDelegate ButtonClick;


  // Update is called once per frame
  void Update()
  {
    SimulateFlashLight();
    SimulateFreq();
    SimulateButton();
   
    TickFlashLightTimer();
    HandleLightEvents();
    HandleButtonEvent();
  }
  
  // Invoked when a line of data is received from the serial device.
  void OnMessageArrived(string msg)
  {
    if (msg.Substring(0, 4) == "FREQ")
    {
      _lastFreqInput = int.Parse(msg.Substring(5, msg.Length - 5));
      FrequencyEvent?.Invoke((int)_lastFreqInput);
      return;
    }
    
    if (msg.Substring(0, 4) == "LIGH")
    {
      _currentButtonInput = int.Parse(msg.Substring(5, msg.Length - 5)) == 1;
      return;
    }
  }
  
  void SimulateFreq()
  {
    if (!simulateFreqInput)
      return;

    _lastFreqInput += Input.mouseScrollDelta.y;
    _lastFreqInput = Mathf.Clamp(_lastFreqInput, 0, 50);
    FrequencyEvent?.Invoke((int)_lastFreqInput);
  }

  void SimulateButton()
  {
    if (!simulateButtonInput)
      return;
    
    _currentButtonInput = Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.D);
  }

  void SimulateFlashLight()
  {
    if (!simulateLightInput)
      return;
    
    _lightInput = Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.F);
  }

  void TickFlashLightTimer()
  {
    if (!_lightInput)
      return;
      
    _currentLightTimer += Time.unscaledDeltaTime;
  }

  void HandleLightEvents()
  {
    if (!_lightInput)
    {
      if (_currentLightTimer <= 0.0f)
        return;
      
      if (_currentLightTimer < flashLightInputTimer)
      {
        PhotoEvent?.Invoke();
      }
      
      FlashLightEvent?.Invoke(false);
      MovementEvent?.Invoke(false);
      
      _currentLightTimer = 0.0f;
      _hasFiredMovementEvent = false;
      _hasFiredFlashLightEvent = false;
      return;
    }
    
    if (!_hasFiredFlashLightEvent)
    {
      FlashLightEvent?.Invoke(true);
      _hasFiredFlashLightEvent = true;
    }
    
    if (_currentLightTimer < flashLightInputTimer || _hasFiredMovementEvent)
      return;

    MovementEvent?.Invoke(true);
    _hasFiredMovementEvent = true;
  }

  void HandleButtonEvent()
  {
    if (_currentButtonInput == _lastButtonInput)
      return;

    if (_currentButtonInput)
    {
      ButtonClick?.Invoke();
    }

    _lastButtonInput = _currentButtonInput;
  }

  public int GetFrequency()
  {
    return (int)_lastFreqInput;
  }
}
