using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour
{
    // Start is called before the first frame update
    public void DisableThisObject()
    {
        gameObject.SetActive(false);
    }
}
