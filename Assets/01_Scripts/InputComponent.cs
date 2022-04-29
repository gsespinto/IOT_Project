using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IOT_Delegates;

public class InputComponent : MonoBehaviour
{
  [SerializeField] private float flashLightInputTimer = 0.25f;
  private float _currentLightTimer = 0.0f;
  private bool _lightInput = false;

  private bool _hasFiredFlashLightEvent = false;
  private bool _hasFiredMovementEvent = false;

  public BoolDelegate FlashLightEvent;
  public BoolDelegate MovementEvent;
  public NoParamsDelegate PhotoEvent;


  // Update is called once per frame
  void Update()
  {
    GetFlashLightInput();
    TickFlashLightTimer();
    HandleLightEvents();
  }

  void GetFlashLightInput()
  {
    _lightInput = Input.GetKey(KeyCode.Mouse0);
  }

  void TickFlashLightTimer()
  {
    if (!_lightInput)
      return;
      
    _currentLightTimer += Time.deltaTime;
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
}
