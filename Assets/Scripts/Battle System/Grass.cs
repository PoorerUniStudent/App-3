using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField] private Encounter[] enemiesInGrass;

    public Encounter[] GetEnemiesInGrass()
    {
        return enemiesInGrass;
    }
}
