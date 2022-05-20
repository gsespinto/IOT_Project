using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private int sceneToLoad;
    [SerializeField] private Animator loadingAnimator;

    void Awake()
    {
        // If there's a loading screen already
        // Destroy this one
        if (GameObject.FindObjectsOfType<SceneLoader>().Length > 1)
            Destroy(this.gameObject);

        // Make this object always loaded
        DontDestroyOnLoad(this);
    }

    /// <summary> Queues scene to load and start loading screen </summary>
    public void LoadScene(int sceneIndex)
    {
        sceneToLoad = sceneIndex;
        loadingAnimator.SetTrigger("LoadScene");
    }

    /// <summary> Starts loading scene to load asynchronously </summary>
    public void StartLoadingScene()
    {
        // Load scene asynchronously
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        // Hide loading screen whenever the new scene is loaded
        loadOperation.completed += StopLoading;
    }

    /// <summary> Triggers loading screen to hide </summary>
    void StopLoading(AsyncOperation obj)
    {
        loadingAnimator.SetTrigger("Unload");
    }   
}
