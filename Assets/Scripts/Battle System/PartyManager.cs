using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PartyMemberInfo[] members;
    [SerializeField] private List<PartyMember> currentParty;

    [SerializeField] private PartyMemberInfo defaultPartyMember;

    private Vector3 playerPosition;
    private static GameObject instance;

    private const int START_EXP_REQ = 8;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = gameObject;
            AddMemberToPartyByName(defaultPartyMember.MemberName);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void AddMemberToPartyByName(string memberName)
    {
        for (int i = 0; i < members.Length; i++)
        {
            if (members[i].MemberName == memberName)
            {
                PartyMember newPartyMember = new PartyMember();

                newPartyMember.MemberName = members[i].MemberName;
                newPartyMember.Level = members[i].StartingLevel; // Change to load data later
                newPartyMember.MaxExp = START_EXP_REQ + (int)Mathf.Pow(newPartyMember.Level, 2);
                newPartyMember.CurrentExp = 0; // Change to load data later
                newPartyMember.MaxHealth = members[i].BaseHealth + (Mathf.Clamp(members[i].BaseHealth / 5, 1, 10) * (newPartyMember.Level - 1));
                newPartyMember.CurrentHealth = newPartyMember.MaxHealth; // load
                newPartyMember.Strength = members[i].BaseStr + (Mathf.Clamp(members[i].BaseStr/ 5, 1, 10) * (newPartyMember.Level - 1));
                newPartyMember.Speed = members[i].BaseSpeed + (Mathf.Clamp(members[i].BaseSpeed / 5, 1, 10) * (newPartyMember.Level - 1));
                newPartyMember.MemberBattleVisualPrefab = members[i].MemberBattleVisualPrefab;
                newPartyMember.MemberOverworldVisualPrefab = members[i].MemberOverworldVisualPrefab;
                newPartyMember.ID = i;
                newPartyMember.Attacks = members[i].Attacks;

                currentParty.Add(newPartyMember);
            }
        }
    }

    public List<PartyMember> GetAliveParty()
    {
        List<PartyMember> aliveParty = new List<PartyMember>(); 
        aliveParty = currentParty;
        for (int i = 0; i < aliveParty.Count; i++)
        {
            if (aliveParty[i].CurrentHealth <= 0)
            {
                aliveParty.RemoveAt(i);
            }
        }
        return aliveParty;
    }

    public List<PartyMember> GetCurrentParty()
    {
        return currentParty;
    }

    public void SaveHealth(int partyMember, int health)
    {
        currentParty[partyMember].CurrentHealth = health;
    }

    public void SetPosition(Vector3 position)
    {
        playerPosition = position;
    }

    public Vector3 GetPosition()
    {
        return playerPosition;
    }

    public void AddExpByID(int id, int exp)
    {
        for (int i = 0; i < currentParty.Count; i++)
        {
            if (currentParty[i].ID != id)
            {
                continue;
            }

            AddExp(i, exp);
            return;
        }
    }

    private void AddExp(int i, int exp)
    {
        currentParty[i].CurrentExp += exp;

        if (currentParty[i].CurrentExp >= currentParty[i].MaxExp)
        {
            currentParty[i].Level += 1;
            currentParty[i].CurrentExp -= currentParty[i].MaxExp;
            Debug.Log(currentParty[i].Level ^ 2);
            currentParty[i].MaxExp = START_EXP_REQ + (int)Mathf.Pow(currentParty[i].Level, 2);

            // Add bonus stats
            int bonusHealth = Mathf.Clamp(members[i].BaseHealth / 5, 1, 10);
            int bonusStrength = Mathf.Clamp(members[i].BaseStr / 5, 1, 10);
            int bonusSpeed = Mathf.Clamp(members[i].BaseSpeed / 5, 1, 10);

            currentParty[i].MaxHealth += bonusHealth;
            currentParty[i].CurrentHealth += bonusHealth;
            currentParty[i].Strength += bonusStrength;
            currentParty[i].Speed += bonusSpeed;

            AddExp(i, 0);
        }
    }

    public int GetLevelByID(int id)
    {
        for (int i = 0; i < currentParty.Count; i++)
        {
            if (currentParty[i].ID == id)
            {
                return currentParty[i].Level;
            }
        }

        return 0;
    }

    public PartyMember GetPartyMemberByID(int id)
    {
        for (int i = 0; i < currentParty.Count; i++)
        {
            if (currentParty[i].ID == id)
            {
                return currentParty[i];
            }
        }

        return null;
    }

    public void ResetPartyMembers()
    {
        for (int i = 0; i < currentParty.Count; i++)
        {
            currentParty[i].CurrentHealth = currentParty[i].MaxHealth;
        }
    }
}

[System.Serializable]
public class PartyMember
{
    public string MemberName;

    public int Level;
    public int CurrentHealth;
    public int MaxHealth;
    public int Strength;
    public int Speed;
    public int CurrentExp;
    public int MaxExp;

    public int ID;
    public MemberAttack[] Attacks;

    public GameObject MemberBattleVisualPrefab;
    public GameObject MemberOverworldVisualPrefab;
}
