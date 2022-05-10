using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ArduinoButton : MonoBehaviour
{
    [SerializeField]
    public ButtonEvent onClick;
    [Space(10)]
    [SerializeField] private Vector2 selectionRange;
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color selectedColor = Color.white;
    private InputComponent _inputComponent;
    private Image _image;
    private ArdityEventSystem _eventSystem;
    
    // Start is called before the first frame update
    void Start()
    {
        _image = this.GetComponent<Image>();
        _inputComponent = GameObject.FindObjectOfType<InputComponent>();
        _eventSystem = GameObject.FindObjectOfType<ArdityEventSystem>();

        _inputComponent.ButtonClick += Click;
    }

    private void Update()
    {
        if (_inputComponent.GetFrequency() >= selectionRange.x && _inputComponent.GetFrequency() <= selectionRange.y)
        {
            if (_eventSystem.CurrentSelectedObj == this.gameObject)
                return;
            
            _eventSystem.CurrentSelectedObj = this.gameObject;
            Debug.Log(name + "Button selected");
            SetButtonColor(selectedColor);
        }
        else
        {
            if (_eventSystem.CurrentSelectedObj == this.gameObject)
            {
                _eventSystem.CurrentSelectedObj = null;
                Debug.Log(name + "Button deselected");
            }
            SetButtonColor(normalColor);
        }
    }

    void Click()
    {
        if (_eventSystem.CurrentSelectedObj != this.gameObject)
            return;
        
        _eventSystem.CurrentSelectedObj = null;
        onClick?.Invoke();
        Debug.Log("Clicked on " + name);
    }

    void SetButtonColor(Color c)
    {
        _image.color = c;
    }
}

[System.Serializable]
public class ButtonEvent : UnityEvent{}
