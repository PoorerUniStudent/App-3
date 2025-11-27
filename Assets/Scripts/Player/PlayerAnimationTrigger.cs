using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationTrigger : MonoBehaviour
{
    private PlayerController playerController;
    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }
    public void LandingFinishedTrigger()
    {
        if (playerController)
        {
            playerController.LandingFinishedTrigger();
        }
    }

    public void JumpTrigger()
    {
        if (playerController)
        {
            playerController?.JumpTrigger();
        }
    }
}
