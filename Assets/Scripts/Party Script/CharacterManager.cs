using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject joinPopup;
    [SerializeField] private TextMeshProUGUI joinPopupText;
    private bool infrontOfPartyMember;
    private GameObject joinableMember;
    private PlayerControls playerControls;


    private const string PARTY_JOINED_MESSAGE = " Joined The Party";
    private const string NPC_JOINABLE_TAG = "NPCJoinable";
    // Start is called before the first frame update

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    void Start()
    {
        playerControls.Player.Interact.performed += _ => Interact();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void Interact()
    {
        if(infrontOfPartyMember == true && joinableMember != null)
        {
            MemberJoined(joinableMember.GetComponent<JoinableCharacterScript>().MemberToJoin);
            infrontOfPartyMember = false;
            joinableMember = null;
        }
    }

    private void MemberJoined(PartyMemberInfo partyMember)
    {
        GameObject.FindFirstObjectByType<PartyManager>().AddMemberToPartyByName(partyMember.MemberName);
        joinableMember.GetComponent<JoinableCharacterScript>().CheckIfJoined();
        joinPopup.SetActive(true);
        joinPopupText.text = partyMember.MemberName + PARTY_JOINED_MESSAGE;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == NPC_JOINABLE_TAG)
        {
            infrontOfPartyMember = true;
            joinableMember = other.gameObject;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == NPC_JOINABLE_TAG)
        {
            infrontOfPartyMember = false;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(true);
            joinableMember = null;
        }
    }
}
