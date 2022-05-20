using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public static class SaveSystem
{
    public static void SaveGame (GameManager gameManager)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/saveData.bin";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveData data = new SaveData(gameManager);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData LoadGame ()
    {
        string path = Application.persistentDataPath + "/saveData.bin";
        Debug.Log(path);

        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found in " + path);
            return null;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open);

        SaveData data = formatter.Deserialize(stream) as SaveData;

        return data;
    }

    public static void DeleteSaveFile()
    {
        string path = Application.persistentDataPath + "/saveData.bin";
        File.Delete(path);
    }

    public static bool HasSaveFile()
    {
        string path = Application.persistentDataPath + "/saveData.bin";
        return File.Exists(path);
    }
}

[System.Serializable]
public class SaveData
{
    public int highScore;

    public SaveData(GameManager gameManager)
    {
        highScore = gameManager.HighScore;
    }
}
