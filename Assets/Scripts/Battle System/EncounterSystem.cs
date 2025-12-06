using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterSystem : MonoBehaviour
{
    [SerializeField] private int maxNumEnemies;

    private EnemyManager enemyManager;
    void Start()
    {
        enemyManager = FindFirstObjectByType<EnemyManager>();
    }

    public void GenerateEncounter(Encounter[] enemiesInGrass)
    {
        enemyManager.GenerateEnemiesByEncounter(enemiesInGrass, maxNumEnemies);
    }
}

[System.Serializable]
public class Encounter
{
    public EnemyInfo Enemy;
    public int LevelMin;
    public int LevelMax;
}
