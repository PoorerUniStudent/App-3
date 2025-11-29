using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PartyMemberInfo[] members;
    [SerializeField] private List<PartyMember> currentParty;

    [SerializeField] private PartyMemberInfo defaultPartyMember;

    private void Awake()
    {
        AddMemberToPartyByName(defaultPartyMember.MemberName);
    }

    public void AddMemberToPartyByName(string memberName)
    {
        for (int i = 0; i < members.Length; i++)
        {
            if (members[i].MemberName == memberName)
            {
                PartyMember newPartyMember = new PartyMember();

                newPartyMember.MemberName = members[i].MemberName;
                newPartyMember.Level = members[i].StartingLevel;
                newPartyMember.CurrentHealth = members[i].BaseHealth;
                newPartyMember.MaxHealth = newPartyMember.CurrentHealth;
                newPartyMember.Strength = members[i].BaseStr;
                newPartyMember.Speed = members[i].BaseSpeed;
                newPartyMember.MemberBattleVisualPrefab = members[i].MemberBattleVisualPrefab;
                newPartyMember.MemberOverworldVisualPrefab = members[i].MemberOverworldVisualPrefab;

                currentParty.Add(newPartyMember);
            }
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

    public GameObject MemberBattleVisualPrefab;
    public GameObject MemberOverworldVisualPrefab;
}
