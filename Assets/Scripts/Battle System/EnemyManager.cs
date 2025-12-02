using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyInfo[] enemies;
    [SerializeField] private List<Enemy> currentEnemies;

    private const float LEVEL_MOD = 0.5f;

    private void Awake()
    {
        GenerateEnemyByName("Blue Slime", 1);
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

                currentEnemies.Add(newEnemy);
            }
        }
    }

    public List<Enemy> GetCurrentEnemies()
    {
        return currentEnemies;
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

    public GameObject EnemyVisualPrefab;
}
