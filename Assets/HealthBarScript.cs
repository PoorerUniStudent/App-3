using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SingleHeroHPUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Slider hpSlider;

    [SerializeField] private int memberID = 0;

    private PartyManager partyManager;

    private void Start()
    {
        // Safely find the PartyManager in the scene
        partyManager = FindFirstObjectByType<PartyManager>();

        if (partyManager == null)
        {
            Debug.LogError("PartyManager not found in scene!");
            return;
        }

        RefreshHP();
    }

    public void RefreshHP()
    {
        // Get the correct member by ID
        PartyMember member = partyManager.GetPartyMemberByID(memberID);
        if (member == null)
        {
            Debug.LogWarning($"Party member with ID {memberID} not found!");
            return;
        }

        hpSlider.maxValue = member.MaxHealth;
        hpSlider.value = member.CurrentHealth;
        hpText.text = $"HP: {member.CurrentHealth} / {member.MaxHealth}";
    }
}