using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputComponent))]
[RequireComponent(typeof(Player))]
public class MenuManager : MonoBehaviour
{
    private bool _gamePaused = true;
    [Header("COMPONENTS")]
    [SerializeField] private Animator playerAnimator;
    private InputComponent _inputComponent;
    private Player _player;
    [SerializeField] private Image photoImage;
    [SerializeField] private Image photoValidationImage;
    [SerializeField] private TextMeshProUGUI photoStorageText;
    [SerializeField] private TextMeshProUGUI selectedPhotoText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [Header("TABS")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject mainTab;
    [SerializeField] private GameObject pauseTab;
    [Header("SPRITES")]
    [SerializeField] private Sprite missingPhotoSprite;
    [SerializeField] private Sprite validPhotoSprite;
    [SerializeField] private Sprite invalidPhotoSprite;

    private void Start()
    {
        _inputComponent = this.GetComponent<InputComponent>();
        _inputComponent.ButtonClick += PauseInput;
        _inputComponent.FrequencyEvent += UpdatePhotoToFrequency;

        _player = this.GetComponent<Player>();
        _player.OnValidPhoto += UpdateScoreText;
        
        UpdateScoreText();
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
        UpdatePhotoToFrequency(_inputComponent.GetFrequency());
        photoStorageText.text = _player.GetPhotoStorageString();
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        _gamePaused = false;
        _player.ReturnFlashLightInput();
        GoToGameTab();
    }

    void UpdatePhotoToFrequency(int freq)
    {
        if (!_gamePaused)
            return;
        
        SetPhotoImage(_player.GetPhoto(freq));
        selectedPhotoText.text = (freq + 1).ToString();
    }
    
    void SetPhotoImage(Photo photo)
    {
        if (photo == null)
        {
            photoImage.sprite = missingPhotoSprite;
            photoValidationImage.gameObject.SetActive(false);
            return;
        }

        photoValidationImage.sprite = photo.IsValid ? validPhotoSprite : invalidPhotoSprite;
        photoValidationImage.gameObject.SetActive(true);
        photoImage.sprite = photo.Image;
    }

    void UpdateScoreText()
    {
        scoreText.text = "x" + _player.GetPhotographedMonstersAmount();
    }
}
