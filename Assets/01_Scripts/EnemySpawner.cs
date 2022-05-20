using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] monsterPrefabs;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (monsterPrefabs.Length <= 0)
        {
            Debug.LogError("Missing monster prefabs in Enemy Spawner", this);
            return;
        }
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int monsterIndex = UnityEngine.Random.Range(0, monsterPrefabs.Length);
            Enemy e = GameObject.Instantiate(monsterPrefabs[monsterIndex], spawnPoints[i].position,
                monsterPrefabs[monsterIndex].transform.rotation).GetComponent<Enemy>();
            
            if (i < spawnPoints.Length - 1)
                e.SetDeathPos(spawnPoints[i + 1].position);
        }
    }
}
