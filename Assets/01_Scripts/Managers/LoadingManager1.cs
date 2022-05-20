using UnityEngine;

public class LoadingManager1 : MonoBehaviour
{
    [SerializeField] private int sceneIndex;
    /// <summary> Loads scene with given index </summary>
    public void LoadScene()
    {
        SceneLoader sceneLoader = GameObject.FindObjectOfType<SceneLoader>();
        if(!sceneLoader)
        {
            return;
        }

        sceneLoader.LoadScene(sceneIndex);
    }
}
