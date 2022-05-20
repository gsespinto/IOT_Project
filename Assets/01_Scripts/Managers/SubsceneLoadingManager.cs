using UnityEngine;

public static class SubsceneLoadingManager
{
    /// <summary> Loads subscene with given index </summary>
    public static void LoadSubscene(int index)
    {
        SubsceneManager sceneManager = GameObject.FindObjectOfType<SubsceneManager>();

        // Null ref protection
        if(!sceneManager)
        {
            Debug.LogWarning("Couldn't find valid reference of SubsceneManager script.");
            return;
        }

        sceneManager.LoadSubscene(index);
    }
}
