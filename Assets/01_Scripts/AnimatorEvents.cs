using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IOT_Delegates;

[RequireComponent(typeof(AudioSource))]
public class AnimatorEvents : MonoBehaviour
{
  [SerializeField] private AudioClip[] stepSfxs;
  [SerializeField] private AudioClip[] specialSfx;
  public NoParamsDelegate OnAttack;
  public NoParamsDelegate OnTakePhoto;
  public NoParamsDelegate OnBuzz;

  // COMPONENTS
  private AudioSource audioSource;

  void Start()
  {
    audioSource = this.GetComponent<AudioSource>();
  }

  public void PlaySpecialSfx(int index)
  {
    if (!audioSource)
    {
      Debug.LogWarning("Missing audio source reference in " + this.gameObject.name + " AnimatorEvents component.");
      return;
    }
    
    audioSource.PlayOneShot(specialSfx[index]);
  }
  
  public void PlayStepSFX()
  {
    OnBuzz?.Invoke();
    
    if (!audioSource)
    {
      Debug.LogWarning("Missing audio source reference in " + this.gameObject.name + " AnimatorEvents component.");
      return;
    }

    if (stepSfxs.Length <= 0)
    {
      Debug.LogWarning("Missing step sfx reference in " + this.gameObject.name + " AnimatorEvents component.");
      return;
    }

    int i = Random.Range(0, stepSfxs.Length - 1);
    audioSource.PlayOneShot(stepSfxs[i], 0.35f);
  }

  public void CallAttackEvent()
  {
    OnAttack?.Invoke();
  }

  public void CallPhotoEvent()
  {
    OnTakePhoto?.Invoke();
  }

  public void Buzz()
  {
    OnBuzz?.Invoke();
  }
}
