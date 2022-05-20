using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
public class MenuManager : MonoBehaviour
{
    private bool _gamePaused = true;
    private bool _gameEnded = false;
    private const int SPACE = 10;
    
    
    [Header("COMPONENTS")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private AudioSource audioPreview;
    [SerializeField] private Image[] photoImages;
    [SerializeField] private Image[] photoValidationImages;
    [SerializeField] private TextMeshProUGUI[] selectedPhotoText;
    [SerializeField] private TextMeshProUGUI photoStorageText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image[] volumeSliders;
    [SerializeField] private Image volumeImage;
    [SerializeField] private Image mutedImage;
    [SerializeField] private TextMeshProUGUI startHighScoreText;
    private Player _player;
    private GameManager _gameManager;
    private InputComponent _inputComponent;
    
    [Space(SPACE)]
    [Header("TABS")]
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject mainTab;
    [SerializeField] private GameObject pauseTab;
    [SerializeField] private GameObject gameOverTab;
    [SerializeField] private GameObject[] settingsTabs;
    
    [Space(SPACE)]
    [Header("SPRITES")]
    [SerializeField] private Sprite missingPhotoSprite;
    [SerializeField] private Sprite validPhotoSprite;
    [SerializeField] private Sprite invalidPhotoSprite;

    [Space(SPACE)] [Header("GAME OVER")] 
    [SerializeField] private TextMeshProUGUI reasonText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject newHighScoreObj;

    private void Start()
    {
        _player = this.GetComponent<Player>();
        _player.OnValidPhoto += UpdateScoreText;
        _player.OnPhoto += UpdateStorageText;
        _player.OnGameOver += GameOver;

        _inputComponent = GameObject.FindObjectOfType<InputComponent>();
        _inputComponent.ButtonClick += PauseInput;
        _inputComponent.FrequencyEvent += UpdatePhotoToFrequency;
        _inputComponent.FrequencyEvent += UpdateVolume;
        
        _gameManager = GameObject.FindObjectOfType<GameManager>();
        startHighScoreText.text = "x" + _gameManager.HighScore;
        
        UpdateScoreText();
        UpdateStorageText();
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
        gameOverTab.SetActive(false);
    }

    private void GoToPauseTab()
    {
        pauseTab.SetActive(true);
        mainTab.SetActive(false);
        gameOverTab.SetActive(false);
    }

    private void GoToGameTab()
    {
        pauseTab.SetActive(false);
        mainTab.SetActive(false);
        gameOverTab.SetActive(false);
    }

    private void GoToGameOverTab()
    {
        pauseTab.SetActive(false);
        mainTab.SetActive(false);
        gameOverTab.SetActive(true);
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
        foreach (TextMeshProUGUI spt in selectedPhotoText)
        {
            spt.text = (freq + 1).ToString();
        }
    }
    
    void SetPhotoImage(Photo photo)
    {
        if (photo == null)
        {
            foreach (Image pi in photoImages)
                pi.sprite = missingPhotoSprite;
            
            foreach (Image pv in photoValidationImages)
                pv.gameObject.SetActive(false);
            
            return;
        }

        foreach (Image pv in photoValidationImages)
        {
            pv.sprite = photo.IsValid ? validPhotoSprite : invalidPhotoSprite;
            pv.gameObject.SetActive(true); 
        }

        foreach (Image pi in photoImages)
            pi.sprite = photo.Image;
    }

    void UpdateScoreText()
    {
        scoreText.text = "x" + _player.GetPhotographedMonstersAmount();
    }

    void UpdateStorageText()
    {
        photoStorageText.text = _player.GetPhotoStorageString();
    }

    void UpdateVolume(int i)
    {
        bool leaveFlag = true;
        foreach (GameObject g in settingsTabs)
        {
            if (g.activeInHierarchy)
            {
                leaveFlag = false;
                break;
            }
        }

        if (leaveFlag)
            return;
        
        foreach (Image vs in volumeSliders)
        {
            vs.fillAmount = _player.GetFreqRatio();
        }

        audioPreview.PlayOneShot(audioPreview.clip);
        volumeImage.gameObject.SetActive(_player.GetFreqRatio() > 0);
        mutedImage.gameObject.SetActive(_player.GetFreqRatio() <= 0);

        if (_player.GetFreqRatio() > 0)
        {
            mixer.SetFloat("MasterVolume", MyMath.Map(_player.GetFreqRatio(), 0, 1, -30, 5));
        }
        else
        {
            mixer.SetFloat("MasterVolume", -80);
        }
    }

    void GameOver(string reason)
    {
        if (_gameEnded)
            return;
        
        reasonText.text = reason;
        bool isNewHighScore = _gameManager.TryToSetHighScore(_player.GetPhotographedMonstersAmount());
        Debug.Log("Is new highScore: " + isNewHighScore);
        newHighScoreObj.SetActive(isNewHighScore);
        finalScoreText.text = "x" + _player.GetPhotographedMonstersAmount();
        highScoreText.text = "x" + _gameManager.HighScore;
        
        TriggerPauseGame(true);
        GoToGameOverTab();

        _gameEnded = true;
    }

    public void RestartGame()
    {
        LoadingManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
