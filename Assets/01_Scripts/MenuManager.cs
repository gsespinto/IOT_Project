using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputComponent))]
[RequireComponent(typeof(Player))]
public class MenuManager : MonoBehaviour
{
    private InputComponent _inputComponent;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject mainTab;
    [SerializeField] private GameObject pauseTab;
    private Player _player;
    private bool _gamePaused = true;

    private void Start()
    {
        _inputComponent = this.GetComponent<InputComponent>();
        _inputComponent.ButtonClick += PauseInput;

        _player = this.GetComponent<Player>();
        
        GoToMainTab();
    }

    public void StartGame()
    {
        TriggerPauseGame(false);
        GoToPauseTab();
    }
    
    private void GoToMainTab()
    {
        mainTab.SetActive(true);
        pauseTab.SetActive(false);
    }

    private void GoToPauseTab()
    {
        pauseTab.SetActive(true);
        mainTab.SetActive(false);
    }

    private void GoToGameTab()
    {
        pauseTab.SetActive(false);
        mainTab.SetActive(false);
    }

    void PauseInput()
    {
        if (_gamePaused)
            return;
        
        GoToPauseTab();
        TriggerPauseGame(true);
    }

    public void TriggerPauseGame(bool pause)
    {
        playerAnimator.SetBool("InMenu", pause);
        root.SetActive(pause);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        _gamePaused = true;
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        _gamePaused = false;
        _player.ReturnFlashLightInput();
        GoToGameTab();
    }
}
