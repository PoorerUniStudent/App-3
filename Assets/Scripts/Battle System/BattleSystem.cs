 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject[] attackSelectionButtons;
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private GameObject attackSelectionMenu;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomTextPopup;
    [SerializeField] private TextMeshProUGUI bottomText;
    [SerializeField] private ChargeBar chargeBar;

    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private int currentPlayer;

    private const string ACTION_MESSAGE = "'s Action:";
    private const string WIN_MESSAGE = "You have won the battle!";
    private const string LOSE_MESSAGE = "Your party has been slain...";
    private const string RUN_MESSAGE = "You successfully ran away!";
    private const string RUN_FAIL_MESSAGE = "You failed to run away!";
    private const int TURN_DURATION = 2;
    private const int RUN_CHANCE = 60;
    private const string OVERWORLD_SCENE = "OverworldScene";

    private Vector3 RESPAWN_POS = new Vector3(-43.17f, 4f, 13.61f);

    void Start()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        enemyManager = FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
        DetermineBattleOrder();
    }

    private IEnumerator BattleRoutine()
    {
        enemySelectionMenu.SetActive(false);
        state = BattleState.Battle;
        bottomTextPopup.SetActive(true);

        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (state != BattleState.Battle || allBattlers[i].CurrentHealth <= 0)
            {
                continue;
            }
            switch (allBattlers[i].BattleAction)
            {
                case BattleEntities.Action.Attack:
                    // do the attack
                    yield return StartCoroutine(AttackRoutine(i));
                    break;
                case BattleEntities.Action.Run:
                    // run from battle
                    yield return StartCoroutine(RunRoutine());
                    break;
                default:
                    Debug.Log("Error - Incorrect Battle Action");
                    break;
            }
        }

        RemoveDeadBattlers();

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
            chargeBar.gameObject.SetActive(true);
            chargeBar.SetCurrentlyAttackingMember(i);
            if (allBattlers[i].CurrentAttack.AttackName == "Slash")
            {
                allBattlers[i].BattleVisuals.PlayAttackAnimation();
            }

            allBattlers[i].DoneTurn = false;
            while (!allBattlers[i].DoneTurn)
            {
                
                yield return new WaitForSeconds(0.1f);
            }

            // attack selected enemy(attack action)
            chargeBar.gameObject.SetActive(false);
            BattleEntities currentAttacker = allBattlers[i];
            if (allBattlers[currentAttacker.Target].CurrentHealth <= 0)
            {
                currentAttacker.SetTarget(GetRandomEnemy());
            }
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

                if (currentAttacker.IsPlayer)
                {
                    int oldLevel = partyManager.GetLevelByID(currentAttacker.ID);
                    partyManager.AddExpByID(currentAttacker.ID, currentTarget.ExpGive);
                    int newLevel = partyManager.GetLevelByID(currentAttacker.ID);
                    bottomText.text = string.Format("{0} gained {1} experience!", currentAttacker.Name, currentTarget.ExpGive);
                    // wait some seconds
                    yield return new WaitForSeconds(TURN_DURATION / 2);

                    if (oldLevel < newLevel)
                    {
                        PartyMember newLeveledPlayer = partyManager.GetPartyMemberByID(currentAttacker.ID);
                        currentAttacker.Level = newLeveledPlayer.Level;
                        currentAttacker.Strength = newLeveledPlayer.Strength;
                        currentAttacker.MaxHealth = newLeveledPlayer.MaxHealth;
                        currentAttacker.CurrentHealth = newLeveledPlayer.CurrentHealth;
                        currentAttacker.UpdateUI();
                        bottomText.text = string.Format("{0} is now level {1}!", currentAttacker.Name, newLevel);
                        DetermineBattleOrder();
                        // wait some seconds
                        yield return new WaitForSeconds(TURN_DURATION / 2);
                    }
                }

                enemyBattlers.Remove(currentTarget);

                if (enemyBattlers.Count <= 0)
                {
                    state = BattleState.Won;
                    bottomText.text = WIN_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    SceneManager.LoadScene(OVERWORLD_SCENE);
                }
            }
        }

        // enemy's turn
        if (i < allBattlers.Count && !allBattlers[i].IsPlayer)
        {
            // attack selected party member
            BattleEntities currentAttacker = allBattlers[i];
            currentAttacker.SetTarget(GetRandomPartyMember());
            BattleEntities currentTarget = allBattlers[currentAttacker.Target];
            AttackAction(currentAttacker, currentTarget);
            // wait a few seconds
            yield return new WaitForSeconds(TURN_DURATION);

            // kill the party member
            if (currentTarget.CurrentHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}!", currentAttacker.Name, currentTarget.Name);
                // wait some seconds
                yield return new WaitForSeconds(TURN_DURATION);

                playerBattlers.Remove(currentTarget);

                // if no part members remain, we lost
                if (playerBattlers.Count <= 0)
                {
                    state = BattleState.Lost;
                    bottomText.text = LOSE_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    partyManager.SetPosition(RESPAWN_POS);
                    partyManager.ResetPartyMembers();
                    SceneManager.LoadScene(OVERWORLD_SCENE);
                }
            }
        }
    }

    private IEnumerator RunRoutine()
    {
        if (state == BattleState.Battle)
        {
            if (Random.Range(1, 101) >= RUN_CHANCE)
            {
                // we run
                bottomText.text = RUN_MESSAGE;
                state = BattleState.Run;
                allBattlers.Clear();
                yield return new WaitForSeconds(TURN_DURATION);
                SceneManager.LoadScene(OVERWORLD_SCENE);
                yield break;
            }
            else
            {
                bottomText.text = RUN_FAIL_MESSAGE;
                yield return new WaitForSeconds(TURN_DURATION);
            }
        }
    }

    public void SetTurnDoneByIndex(int i)
    {
        allBattlers[i].DoneTurn = true;
    }

    private void RemoveDeadBattlers()
    {
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].CurrentHealth <= 0)
            {
                allBattlers.RemoveAt(i);
            }
        }
    }

    private void CreatePartyEntities()
    {
        // get current party
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetAliveParty();

        // create battle entities for our party members
        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();

            tempEntity.SetEntityValues(currentParty[i].MemberName, currentParty[i].CurrentHealth, 
            currentParty[i].MaxHealth, currentParty[i].Speed, currentParty[i].Strength, currentParty[i].Level, true);
            tempEntity.SetID(currentParty[i].ID);
            tempEntity.Attacks = currentParty[i].Attacks;

            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].MemberBattleVisualPrefab, 
            partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentParty[i].CurrentHealth, currentParty[i].MaxHealth, currentParty[i].Level); 
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
            tempEntity.ExpGive = currentEnemies[i].ExpGive;

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
        attackSelectionMenu.SetActive(false);
        enemySelectionMenu.SetActive(true);
        SetEnemySelectionButtons();
    }

    public void ShowAttackSelectionMenu()
    {
        battleMenu.SetActive(false);
        attackSelectionMenu.SetActive(true);
        SetAttackSelectionButtons();
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

    private void SetAttackSelectionButtons()
    {
        for (int i = 0; i < attackSelectionButtons.Length; i++)
        {
            if (i >= attackSelectionButtons.Length)
            {
                attackSelectionButtons[i].SetActive(false);
            }
            else
            {
                attackSelectionButtons[i].SetActive(true);
                attackSelectionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = playerBattlers[currentPlayer].Attacks[i].AttackName; // attack name
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

    public void SelectAttack(int attackIndex)
    {
        playerBattlers[currentPlayer].CurrentAttack = playerBattlers[currentPlayer].Attacks[attackIndex];

        ShowEnemySelectionMenu();
    }

    private void AttackAction(BattleEntities currentAttacker, BattleEntities currentTarget)
    {
        int damage = currentAttacker.Strength;
        if (currentAttacker.IsPlayer)
        {
            damage = (int)(damage * chargeBar.damageMod * currentAttacker.CurrentAttack.AttackMult);
            if (currentAttacker.CurrentAttack.AttackName == "Shoot")
            {
                currentAttacker.BattleVisuals.PlayShootAnimation();
            }
            else
            {
                currentAttacker.BattleVisuals.PlayAttackReleaseAnimation();
            }
        }
        else
        {
            currentAttacker.BattleVisuals.PlayAttackAnimation();
        }

        damage = Mathf.Clamp(damage, 1, 999);
        
        currentTarget.CurrentHealth -= damage;
        currentTarget.BattleVisuals.PlayHitAnimation();
        currentTarget.UpdateUI();

        bottomText.text = string.Format("{0} attacks {1} for {2} damage!", currentAttacker.Name, currentTarget.Name, damage);
        SaveHealth();
    }

    private int GetRandomPartyMember()
    {
        List<int> partyMembers = new List<int>();

        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == true && allBattlers[i].CurrentHealth > 0)
            {
                partyMembers.Add(i);
            }
        }

        return partyMembers[Random.Range(0, partyMembers.Count)];
    }

    private int GetRandomEnemy()
    {
        List<int> enemies = new List<int>();

        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (!allBattlers[i].IsPlayer && allBattlers[i].CurrentHealth > 0)
            {
                enemies.Add(i);
            }
        }

        return enemies[Random.Range(0, enemies.Count)];
    }

    private void SaveHealth()
    {
        for (int i = 0; i < playerBattlers.Count; i++)
        {
            partyManager.SaveHealth(i, playerBattlers[i].CurrentHealth);
        }
    }

    private void DetermineBattleOrder()
    {
        allBattlers.Sort((bi1, bi2) => -bi1.Speed.CompareTo(bi2.Speed)); // sorts list by speed in ascending order
    }

    public void SelectRunAction()
    {
        state = BattleState.Selection;
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];

        currentPlayerEntity.BattleAction = BattleEntities.Action.Run;

        battleMenu.SetActive(false);
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
    public int ID;
    public int ExpGive; // only for enemies
    public MemberAttack[] Attacks;
    public MemberAttack CurrentAttack;
    public bool IsPlayer;
    public int Target;

    public BattleVisuals BattleVisuals;

    public bool DoneTurn; // Only for players

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
        BattleVisuals.ChangeLevel(Level);
    }

    // For players only
    public void SetID(int id)
    {
        ID = id;
    }

    public void SetExpGive(int expGive)
    {
        ExpGive = expGive;
    }
}
