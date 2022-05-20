using UnityEngine;

public static class LoadingManager
{
    /// <summary> Loads scene with given index </summary>
    public static void LoadScene(int sceneIndex)
    {
        SceneLoader sceneLoader = GameObject.FindObjectOfType<SceneLoader>();
        if(!sceneLoader)
        {
            return;
        }

        sceneLoader.LoadScene(sceneIndex);
    }
}
