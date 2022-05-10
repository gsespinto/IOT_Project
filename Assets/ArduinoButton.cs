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
    private EventSystem _eventSystem;
    
    // Start is called before the first frame update
    void Start()
    {
        _image = this.GetComponent<Image>();
        _inputComponent = GameObject.FindObjectOfType<InputComponent>();
        _eventSystem = GameObject.FindObjectOfType<EventSystem>();

        _inputComponent.ButtonClick += Click;
    }

    private void Update()
    {
        if (_inputComponent.GetFrequency() >= selectionRange.x && _inputComponent.GetFrequency() <= selectionRange.y)
        {
            if (_eventSystem.currentSelectedGameObject == this.gameObject)
                return;
            
            _eventSystem.SetSelectedGameObject(this.gameObject);
            Debug.Log(name + "Button selected");
            SetButtonColor(selectedColor);
        }
        else
        {
            if (_eventSystem.currentSelectedGameObject == this.gameObject)
            {
                _eventSystem.SetSelectedGameObject(null);
                Debug.Log(name + "Button deselected");
            }
            SetButtonColor(normalColor);
        }
    }

    void Click()
    {
        if (_eventSystem.currentSelectedGameObject != this.gameObject)
            return;
        
        _eventSystem.SetSelectedGameObject(null);
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
