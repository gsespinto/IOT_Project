using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int _highScore;

    private void Awake()
    {
        if (GameObject.FindObjectsOfType<GameManager>().Length > 1)
            Destroy(this);

        if (SaveSystem.HasSaveFile())
            _highScore = SaveSystem.LoadGame().highScore;
    }

    public bool TryToSetHighScore(int finalScore)
    {
        if (finalScore <= _highScore)
            return false;
        
        _highScore = finalScore;
        SaveSystem.SaveGame(this);
        
        return true;
    }

    public int HighScore
    {
        get { return _highScore; }
    }
}