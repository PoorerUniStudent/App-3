using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private enum BattleState { Start, Selection, Battle, Won, Lost, Run }

    [Header("Battle States")]
    [SerializeField] private BattleState state;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();

    [Header("UI")]
    [SerializeField] private GameObject[] enemySelectionButtons;
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomTextPopup;
    [SerializeField] private TextMeshProUGUI bottomText;

    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private int currentPlayer;

    private const string ACTION_MESSAGE = "'s Action:";
    private const string WIN_MESSAGE = "You have won the battle!";
    private const int TURN_DURATION = 2;

    void Start()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        enemyManager = FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
    }

    private IEnumerator BattleRoutine()
    {
        enemySelectionMenu.SetActive(false);
        state = BattleState.Battle;
        bottomTextPopup.SetActive(true);

        for (int i = 0; i < allBattlers.Count; i++)
        {
            switch (allBattlers[i].BattleAction)
            {
                case BattleEntities.Action.Attack:
                    // do the attack
                    yield return StartCoroutine(AttackRoutine(i));
                    break;
                case BattleEntities.Action.Run:
                    // run from battle
                    break;
                default:
                    Debug.Log("Error - Incorrect Battle Action");
                    break;
            }
        }

        if (state == BattleState.Battle)
        {
            bottomTextPopup.SetActive(false);
            currentPlayer = 0;
            ShowBattleMenu();
        }

        yield return null;
    }

    private IEnumerator AttackRoutine(int i)
    {
        // player's turn
        if (allBattlers[i].IsPlayer)
        {
            // attack selected enemy (attack action)
            BattleEntities currentAttacker = allBattlers[i];
            BattleEntities currentTarget = allBattlers[currentAttacker.Target];
            AttackAction(currentAttacker, currentTarget);
            // wait some seconds
            yield return new WaitForSeconds(TURN_DURATION);

            // kill the enemy
            if (currentTarget.CurrentHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}!", currentAttacker.Name, currentTarget.Name);
                // wait some seconds
                yield return new WaitForSeconds(TURN_DURATION);

                enemyBattlers.Remove(currentTarget);
                allBattlers.Remove(currentTarget);

                if (enemyBattlers.Count <= 0)
                {
                    state = BattleState.Won;
                    bottomText.text = WIN_MESSAGE;
                    Debug.Log("Go back to overworld scene...??");
                }
            }
        }

        // enemy's turn
        // attack selected party member
        // wait a few seconds
        // kill the party member
        // if no part members remain, we lost
    }

    private void CreatePartyEntities()
    {
        // get current party
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetCurrentParty();

        // create battle entities for our party members
        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentParty[i].MemberName, currentParty[i].CurrentHealth, 
            currentParty[i].MaxHealth, currentParty[i].Speed, currentParty[i].Strength, currentParty[i].Level, true);

            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].MemberBattleVisualPrefab, 
            partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentParty[i].MaxHealth, currentParty[i].MaxHealth, currentParty[i].Level); 
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);
        }
    }

    private void CreateEnemyEntities()
    {
        List<Enemy> currentEnemies= new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentEnemies[i].EnemyName, currentEnemies[i].CurrentHealth,
            currentEnemies[i].MaxHealth, currentEnemies[i].Speed, currentEnemies[i].Strength, currentEnemies[i].Level, false);

            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].EnemyVisualPrefab,
            enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentEnemies[i].MaxHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Level);
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);
        }
    }

    public void ShowBattleMenu()
    {
        actionText.text = playerBattlers[currentPlayer].Name + ACTION_MESSAGE;
        battleMenu.SetActive(true);
    }

    public void ShowEnemySelectionMenu()
    {
        battleMenu.SetActive(false);
        enemySelectionMenu.SetActive(true);
        SetEnemySelectionButtons();
    }

    private void SetEnemySelectionButtons()
    {
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            if (i >= enemyBattlers.Count)
            {
                enemySelectionButtons[i].SetActive(false);
            }
            else
            {
                enemySelectionButtons[i].SetActive(true);
                enemySelectionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = enemyBattlers[i].Name;
            }
        }
    }

    public void SelectEnemy(int currentEnemy)
    {
        playerBattlers[currentPlayer].SetTarget(allBattlers.IndexOf(enemyBattlers[currentEnemy]));

        playerBattlers[currentPlayer].BattleAction = BattleEntities.Action.Attack;
        currentPlayer++;

        if (currentPlayer >= playerBattlers.Count)
        {
            //start battle
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();
        }


    }

    private void AttackAction(BattleEntities currentAttacker, BattleEntities currentTarget)
    {
        int damage = currentAttacker.Strength;
        currentAttacker.BattleVisuals.PlayAttackAnimation();
        currentTarget.CurrentHealth -= damage;
        currentTarget.BattleVisuals.PlayHitAnimation();
        currentTarget.UpdateUI();

        bottomText.text = string.Format("{0} attacks {1} for {2} damage!", currentAttacker.Name, currentTarget.Name, damage);
    }
}

[System.Serializable]
public class BattleEntities
{
    public enum Action 
    {
        Attack,
        Run
    }
    public Action BattleAction;

    public string Name;
    public int CurrentHealth;
    public int MaxHealth;
    public int Speed;
    public int Strength;
    public int Level;
    public bool IsPlayer;
    public int Target;

    public BattleVisuals BattleVisuals;

    public void SetEntityValues(string name, int currentHealth, int maxHealth, int speed, int strength, int level, bool isPlayer)
    {
        Name = name;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Speed = speed;
        Strength = strength;
        Level = level; 
        IsPlayer = isPlayer;
    }

    public void SetTarget(int target)
    {
        Target = target;
    }

    public void UpdateUI()
    {
        BattleVisuals.ChangeHealth(CurrentHealth);
    }
}
