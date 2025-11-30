using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Party Member")]
public class PartyMemberInfo : ScriptableObject
{
    public string MemberName;
    public int StartingLevel;
    public int BaseHealth;
    public int BaseStr;
    public int BaseSpeed;

    public GameObject MemberBattleVisualPrefab; // What will be displayed in battle
    public GameObject MemberOverworldVisualPrefab; // What will be displayed in the overworld scene
}
