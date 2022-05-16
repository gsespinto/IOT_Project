using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using IOT_Delegates;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(InputComponent))]
[RequireComponent(typeof(HealthComponent))]
public class Player : MonoBehaviour
{
  private const int SPACE = 10;
  private const int MAX_PHOTOS = 20;
  
  private Enemy _currentEnemy = null;
  private bool _takingPhoto = false;
  private bool _hasLightInput = false;
  
  [Header("COMPONENTS")]
  [SerializeField] private GameObject flashLightObj;
  private NavMeshAgent _agentComponent;
  private InputComponent _inputComponent;
  private HealthComponent _healthComponent;

  [Space(SPACE)]
  [Header("GAME")] 
  [SerializeField] private float levelTimer = 120.0f;
  private float _currentLevelTimer;
  public StringDelegate OnGameOver;
  [SerializeField] private string deathMessage;
  [SerializeField] private string timerMessage;
  [SerializeField] private string storageMessage;
  
  [Space(SPACE)]
  [Header("MOVEMENT")]
  [SerializeField] private Transform[] pathPoints;
  private int _currentPathPoint = 0;
  private bool _canWalk = true;

  [Space(SPACE)]
  [Header("ANIMATIONS")]
  [SerializeField] private Animator modelAnimator;
  [SerializeField] private Animator lightUIAnimator;
  [SerializeField] private Animator cameraFlashAnimator;
  [SerializeField] private AnimatorEvents cameraFlashEvents;
  [SerializeField] private Animator hudAnimator;
  private bool _hasTriggeredId = false;
  
  [Space(SPACE)]
  [Header("UI")]
  [SerializeField] private Image healthOverlay;
  [SerializeField] private RectTransform idRect;
  [SerializeField] private Image idMeter;
  [SerializeField] private TextMeshProUGUI frequencyTxt;
  [SerializeField] private Image bateryFill;
  [SerializeField] private FBateryColor[] bateryColors;
  [SerializeField] private TextMeshProUGUI bateryText;
  
  [Space(SPACE)]
  [Header("DISH")]
  [SerializeField] private Transform dishRoot;
  [SerializeField] private float dishDamping = 3.0f;
  [SerializeField] private Vector2 dishWaitRange = new Vector2(1.0f, 2.0f);
  private float _dishCurrentWaitTime;
  private Quaternion _dishTargetRot;
  
  [Space(SPACE)]
  [Header("PHOTO")]
  private Texture2D photoTex = null;
  [SerializeField] private Camera photoCamera;
  [SerializeField] private RenderTexture photoRenderTexture;
  [SerializeField] private float validPhotoDistance = 10.0f;
  private float _currentIdTime = 0.0f;
  private float _currentFrequency = 0.0f;
  private bool _isValidPhoto = false;
  private string _currentPlaySession;
  private int _currentPhoto = 0;
  private Photo[] _photos = new Photo[MAX_PHOTOS];
  private Enemy _lastPhotographedEnemy;
  private int _photographedMonsters = 0;
  public NoParamsDelegate OnPhoto;
  public NoParamsDelegate OnValidPhoto;
  
  // Start is called before the first frame update
  void Awake()
  {
    _currentPlaySession = System.DateTime.Now.ToString();
    _currentPlaySession = _currentPlaySession.Replace("/", "_");
    _currentPlaySession = _currentPlaySession.Replace(":", "_");
    
    _agentComponent = this.GetComponent<NavMeshAgent>();
    _inputComponent = this.GetComponent<InputComponent>();
    _healthComponent = this.GetComponent<HealthComponent>();

    _inputComponent.FlashLightEvent += HandleFlashLightInput;
    _inputComponent.MovementEvent += HandleMovement;
    _inputComponent.PhotoEvent += TriggerPhoto;
    _inputComponent.FrequencyEvent += HandleFrequencyInput;

    _healthComponent.OnUpdatedHP += UpdateHpVisuals;
    _healthComponent.OnDeath += GameOverByDeath;
    
    HandleFlashLightInput(false);
    HandleMovement(false);
    WalkPath();
    ChangeHP(0);

    _dishTargetRot = dishRoot.rotation;
    _dishCurrentWaitTime = Random.Range(dishWaitRange.x, dishWaitRange.y);

    if (cameraFlashEvents)
    {
      cameraFlashEvents.OnTakePhoto += TakePhoto;
    }
    else
    {
      Debug.LogWarning("Missing camera flash events reference!", this);
    }

    _currentLevelTimer = levelTimer;
  }

  // Update is called once per frame
  void Update()
  {
    WalkPath();
    AnimatorHandler();
    PointDishAtEnemy();
    IdEnemy();
    TickLevelTimer();
  }

  // ReSharper disable Unity.PerformanceAnalysis
  void WalkPath()
  {
    if (_currentPathPoint >= pathPoints.Length)
    {
      if (!_agentComponent.isStopped)
      {
        Debug.Log("Has reached paths end.");
        _agentComponent.isStopped = true;
      }

      return;
    }

    if (_agentComponent.destination != pathPoints[_currentPathPoint].position)
    {
      _agentComponent.SetDestination(pathPoints[_currentPathPoint].position);
    }

    if (Vector3.Distance(this.transform.position, pathPoints[_currentPathPoint].position) <=
    _agentComponent.stoppingDistance + 1)
    {
      _currentPathPoint++;
    }
  }

  void AnimatorHandler()
  {
    // Null ref protection
    if (!modelAnimator)
    {
      Debug.LogWarning("Missing model animator reference in " + this.gameObject.name + " Player script.");
      return;
    }

    modelAnimator.SetFloat("Speed", (_agentComponent.velocity.magnitude / _agentComponent.speed));
  }

  void HandleFlashLightInput(bool isOn)
  {
    _hasLightInput = isOn;

    if (_takingPhoto)
      return;

    ToggleFlashLight(_hasLightInput);
  }

  void HandleFrequencyInput(int input)
  {
    _currentFrequency = input; // Mathf.Clamp(_currentFrequency + input, 0.0f, 1000.0f);
    Debug.Log("Current frequency: " + (int)_currentFrequency);
  }

  void ToggleFlashLight(bool isOn)
  {
    if (Time.timeScale == 0)
      return;
    
    flashLightObj.SetActive(isOn);
    
    if (lightUIAnimator)
      lightUIAnimator.SetBool("IsLightOn", isOn);
  }

  public void ReturnFlashLightInput()
  {
    ToggleFlashLight(_hasLightInput);
  }

  void HandleMovement(bool canMove)
  {
    _agentComponent.isStopped = !canMove || !_canWalk;
  }

  void TriggerPhoto()
  {
    if (_takingPhoto)
      return;
    
    if (lightUIAnimator)
      lightUIAnimator.SetTrigger("TakePhoto");
    if (cameraFlashAnimator)
      cameraFlashAnimator.SetTrigger("TakePhoto");

    _takingPhoto = true;
    ToggleFlashLight(true);
  }

  void TakePhoto()
  {
    if (_currentPhoto > MAX_PHOTOS)
    {
      OnGameOver?.Invoke(storageMessage);
      return;
    }
    
    if (_currentEnemy)
    {
      if (_lastPhotographedEnemy != _currentEnemy && _isValidPhoto)
      {
        _photographedMonsters++;
        _lastPhotographedEnemy = _currentEnemy;
        OnValidPhoto?.Invoke();
      }
      
      _currentEnemy.GetAttacked(_currentFrequency);
    }

    RenderTexture.active = photoRenderTexture;
    // Read pixels
    Rect photoRect = new Rect(Vector2.zero, new Vector2(photoRenderTexture.width, photoRenderTexture.height));
    photoTex = new Texture2D(photoRenderTexture.width, photoRenderTexture.height);
    photoTex.ReadPixels(photoRect, 0, 0);
    photoTex.Apply();

    /*//then Save To Disk as PNG
    byte[] bytes = photoTex.EncodeToPNG();
    var dirPath = Application.dataPath + "/../SaveImages/" + _currentPlaySession + "/";
    if(!Directory.Exists(dirPath)) {
      Directory.CreateDirectory(dirPath);
    }
    File.WriteAllBytes(dirPath + _currentPhoto + ".png", bytes);
    Debug.Log(dirPath);*/
    
    _photos[_currentPhoto] = new Photo(Sprite.Create(photoTex, photoRect,new Vector2(0,0),1), _isValidPhoto);
    _currentPhoto++;
    
    // Clean up
    RenderTexture.active = null; // added to avoid errors
    
    Debug.Log("Took photo!");
    
    _takingPhoto = false;
    ReturnFlashLightInput();
    Debug.Log("Valid photo: " + _isValidPhoto);
    OnPhoto?.Invoke();
  }

  public void SetCanWalk(bool isPossible)
  {
    _canWalk = isPossible;
    HandleMovement(!_agentComponent.isStopped);
  }

  public bool IsLightOn()
  {
    return flashLightObj.activeSelf;
  }

  public void ChangeHP(float amount)
  {
    _healthComponent.ChangeHP(amount);
  }

  void UpdateHpVisuals()
  {
    Color c = healthOverlay.color;
    c.a = 1 - _healthComponent.HealthRatio();
    healthOverlay.color = c;
  }

  void PointDishAtEnemy()
  {
    if (!dishRoot)
    {
      Debug.LogWarning("Missing reference to dish root!", this);
      return;
    }

    if (!_currentEnemy)
    {
      if (_dishCurrentWaitTime > 0)
      {
        _dishCurrentWaitTime -= Time.deltaTime;
      }
      else
      {
        Vector3 direction = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
        _dishTargetRot = DishLookAtRotation(direction);
        _dishCurrentWaitTime = Random.Range(dishWaitRange.x, dishWaitRange.y);
      }
    }
    else
    {
      Vector3 direction = _currentEnemy.GetIdPosition() - dishRoot.transform.position;
      _dishTargetRot = DishLookAtRotation(direction);
    }

    dishRoot.transform.rotation = Quaternion.Slerp(dishRoot.rotation, _dishTargetRot, Time.deltaTime * dishDamping);;
  }

  Quaternion DishLookAtRotation(Vector3 target)
  {
    target.y = 0;
    return Quaternion.LookRotation(target.normalized);
  }

  public void SetCurrentEnemy(Enemy e)
  {
    _currentEnemy = e;
  }

  void IdEnemy()
  {
    if (!idRect || !photoCamera || !hudAnimator)
      return;

    if (!_currentEnemy || Vector3.Distance(this.transform.position, _currentEnemy.transform.position) > validPhotoDistance)
    {
      hudAnimator.SetBool("ShowID", false);
      _currentIdTime = 0.0f;
      frequencyTxt.gameObject.SetActive(false);
      _isValidPhoto = false;
      return;
    }
    
    Vector2 enemyViewportPos = photoCamera.WorldToViewportPoint(_currentEnemy.GetIdPosition());
    idRect.anchoredPosition = new Vector2(enemyViewportPos.x * photoRenderTexture.width, enemyViewportPos.y * photoRenderTexture.height);
    hudAnimator.SetBool("ShowID", true);
    _currentEnemy.ShowMonster(true);
   
    if (!IsLightOn())
      return;

    _isValidPhoto = true;
    frequencyTxt.gameObject.SetActive(true);
      
    if (_currentIdTime < _currentEnemy.GetIdTime())
    {
      idMeter.transform.parent.gameObject.SetActive(true);
      _currentIdTime += Time.deltaTime;
      idMeter.fillAmount = _currentIdTime / _currentEnemy.GetIdTime();
      frequencyTxt.text = "??? Hz";
    }
    else
    {
      frequencyTxt.text = _currentEnemy.GetFrequency() + " Hz";
      idMeter.transform.parent.gameObject.SetActive(false);
    }
  }

  public Photo GetPhoto(int index)
  {
    index = Mathf.Clamp(index, 0, MAX_PHOTOS - 1);
    return _photos[index];
  }

  public string GetPhotoStorageString()
  {
    return (_currentPhoto) + " / " + MAX_PHOTOS;
  }

  public int GetPhotographedMonstersAmount()
  {
    return _photographedMonsters;
  }

  public float GetFreqRatio()
  {
    return _currentFrequency / MAX_PHOTOS;
  }

  void TickLevelTimer()
  {
    if (_currentLevelTimer <= 0.0f && Time.deltaTime > 0)
    {
      OnGameOver?.Invoke(timerMessage);
      return;
    }
    
    _currentLevelTimer -= Time.deltaTime;

    float percentage = _currentLevelTimer / levelTimer;
    bateryFill.fillAmount = percentage;
    
    bateryText.text = (int)(percentage * 100) + "%";
    
    if (bateryColors.Length < 1)
      return;

    if (percentage <= bateryColors[bateryColors.Length - 1].bateryPercentage)
    {
      bateryFill.color = bateryColors[bateryColors.Length - 1].color;
      return;
    }
    
    FBateryColor targetColor = bateryColors[0];
    FBateryColor currentColor = bateryColors[1];

    for (int i = 0; i < bateryColors.Length - 1; i++)
    {
      if (percentage <= bateryColors[i].bateryPercentage && percentage > bateryColors[i + 1].bateryPercentage)
      {
        currentColor = bateryColors[i];
        targetColor = bateryColors[i + 1];
        break;
      }
    }

    float mappedPercentage = MyMath.Map(percentage, targetColor.bateryPercentage, currentColor.bateryPercentage, 0, 1);
    float lerp = 1 - mappedPercentage;
    Color c = Color.Lerp(currentColor.color, targetColor.color, lerp);
    bateryFill.color = c;
  }

  void GameOverByDeath()
  {
    OnGameOver?.Invoke(deathMessage);
  }
  
  void GameOverByTimer()
  {
    OnGameOver?.Invoke(timerMessage);
  }
  
  void GameOverByStorage()
  {
    OnGameOver?.Invoke(storageMessage);
  }
}

public class Photo
{
  public Photo(Sprite _sprite, bool _valid)
  {
    _image = _sprite;
    _isValid = _valid;
  }
    
  private Sprite _image;
  public Sprite Image
  {
    get { return _image; }
  }
  private bool _isValid;

  public bool IsValid
  {
    get { return _isValid; }
  }
}
