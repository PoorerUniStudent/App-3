using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyInfo[] enemies;
    [SerializeField] private List<Enemy> currentEnemies;

    private static GameObject instance;

    private const float LEVEL_MOD = 0.5f;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = gameObject;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void GenerateEnemyByName(string enemyName, int level)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemyName == enemies[i].EnemyName)
            {
                Enemy newEnemy = new Enemy();

                newEnemy.EnemyName = enemies[i].EnemyName;
                newEnemy.Level = level;
                float levelModifier = LEVEL_MOD * newEnemy.Level;

                newEnemy.MaxHealth = Mathf.RoundToInt(enemies[i].BaseHealth + (enemies[i].BaseHealth * levelModifier));
                newEnemy.CurrentHealth = newEnemy.MaxHealth;
                newEnemy.Strength = Mathf.RoundToInt(enemies[i].BaseStr + (enemies[i].BaseStr * levelModifier));
                newEnemy.Speed = Mathf.RoundToInt(enemies[i].BaseSpeed + (enemies[i].BaseSpeed * levelModifier));
                newEnemy.EnemyVisualPrefab = enemies[i].EnemyVisualPrefab;
                newEnemy.ExpGive = enemies[i].BaseExpGive + (enemies[i].BaseExpGive/5 * (newEnemy.Level-1));

                currentEnemies.Add(newEnemy);
            }
        }
    }

    public List<Enemy> GetCurrentEnemies()
    {
        return currentEnemies;
    }

    public void GenerateEnemiesByEncounter(Encounter[] encounters, int maxNumEnemies)
    {
        currentEnemies.Clear();
        int numEnemies = Random.Range(1, maxNumEnemies + 1);

        for (int i = 0; i < numEnemies; i++)
        {
            Encounter tempEncounter = encounters[Random.Range(0, encounters.Length)];
            int level = Random.Range(tempEncounter.LevelMin, tempEncounter.LevelMax + 1);
            GenerateEnemyByName(tempEncounter.Enemy.EnemyName, level);
        }
    }
}

[System.Serializable]
public class Enemy
{
    public string EnemyName;

    public int Level;
    public int CurrentHealth;
    public int MaxHealth;
    public int Strength;
    public int Speed;
    public int ExpGive;

    public GameObject EnemyVisualPrefab;
}
