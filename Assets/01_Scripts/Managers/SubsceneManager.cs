using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SubsceneManager : MonoBehaviour
{
    private const int SPACE = 10;
    
    [Header("Subcenes")]
    [SerializeField] private Subscene[] subscenes;
    private int subsceneToLoad;
    
    [Header("UI"), Space(SPACE * 2)]
    [SerializeField] private Animator loadingAnimator;
    
    /// <summary> Queues subscene to load and starts loading screen </summary>
    public void LoadSubscene(int subsceneIndex)
    {
        // Null ref protection
        if (!loadingAnimator)
        {
            Debug.LogWarning("Missing loading screen animator reference.", this);
            return;
        }

        // If subscene to load is the one that is already active
        // Do nothing
        if (subsceneToLoad == subsceneIndex)
            return;

        // Setup loading
        subsceneToLoad = subsceneIndex;
        loadingAnimator.SetTrigger("LoadSubscene");
    }

    /// <summary> Activates subscene to load and deactivates all others </summary>
    public void StartLoadingSubscene()
    {
        for (int i = 0; i < subscenes.Length; i++)
        {
            // Activate scene to load
            if (i == subsceneToLoad)
            {
                subscenes[i].subscene.SetActive(true);
                continue;
            }

            // Deactivate all others
            subscenes[i].subscene.SetActive(false);
        }
        
        // Stop loading screen
        loadingAnimator.SetTrigger("Unload");
    }
}

[System.Serializable]
public class Subscene
{
    public GameObject subscene;
    public string name;
}
