using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinableCharacterScript : MonoBehaviour
{
    // Start is called before the first frame update
    public PartyMemberInfo MemberToJoin;
    [SerializeField] private GameObject interactPrompt;
    void Start()
    {
        CheckIfJoined();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowInteractPrompt(bool showPrompt)
    {
        if (showPrompt == true)
        {
            interactPrompt.SetActive(true);
        }
        else
        {
            interactPrompt.SetActive(false);
        }
    }

    public void CheckIfJoined()
    {
        List<PartyMember> currParty = GameObject.FindFirstObjectByType<PartyManager>().GetCurrentParty();
        for (int i = 0; i < currParty.Count; i++)
        {
            if (currParty[i].MemberName == MemberToJoin.MemberName)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
