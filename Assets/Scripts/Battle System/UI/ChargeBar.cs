using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    [SerializeField] private Image aimBar;
    [SerializeField] private Image targetBar;

    private bool isActive;
    private float lerpTime;
    private float maxTargetPos;

    private int currentPartyMember; // index of party member using this

    private Vector3 startPos;
    private Vector3 endPos;

    private PlayerControls playerControls;
    private BattleSystem battleSystem;

    private const int DEFAULT_AIM_BAR_POS = -115;
    private const float LERP_DURATION = 1.5f;

    public float damageMod { get; private set; }


    private void Awake()
    {
        startPos = new Vector3(DEFAULT_AIM_BAR_POS, 0, 0);
        endPos = new Vector3(-DEFAULT_AIM_BAR_POS, 0, 0);
        maxTargetPos = Mathf.Abs(DEFAULT_AIM_BAR_POS) - targetBar.rectTransform.rect.width/2;
    }

    private void Start()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
        }

        aimBar.rectTransform.anchoredPosition = new Vector2(DEFAULT_AIM_BAR_POS, 0);
        float randomTargetPos = Random.Range(-maxTargetPos, maxTargetPos);
        targetBar.rectTransform.anchoredPosition = new Vector2(randomTargetPos, 0);
        playerControls.Enable();
        isActive = true;
    }

    private void Update()
    {
        if (isActive)
        {
            bool input = playerControls.Player.Jump.IsPressed();

            if (input)
            {
                lerpTime += Time.deltaTime;
                Vector2 currentPos = aimBar.rectTransform.anchoredPosition;
                float newX = Mathf.Lerp(startPos.x, endPos.x, lerpTime);
                aimBar.rectTransform.anchoredPosition = new Vector2(newX, currentPos.y);

                if (lerpTime >= LERP_DURATION)
                {
                    isActive = false;
                    lerpTime = 0f;

                    SetToMinDamageMod();
                    battleSystem.SetTurnDoneByIndex(currentPartyMember);
                }
            }
            else if (lerpTime != 0f && !input)
            {
                Debug.Log("RELEASE!!");
                isActive = false;
                lerpTime = 0f;

                float posDiff = (int)(aimBar.rectTransform.position.x - targetBar.rectTransform.position.x - targetBar.rectTransform.rect.width/2);
                float threshold = Mathf.Abs(DEFAULT_AIM_BAR_POS);
                if (aimBar.rectTransform.position.x > targetBar.rectTransform.position.x)
                {
                    posDiff = (int)(aimBar.rectTransform.position.x - targetBar.rectTransform.position.x + targetBar.rectTransform.rect.width / 2);
                }
                posDiff = Mathf.Abs(posDiff);

                Debug.Log(posDiff / threshold);
                if (posDiff / threshold <= 0.25f)
                {
                    damageMod = 1f;
                }
                else
                {
                    damageMod = 1f - posDiff/threshold;
                }

                battleSystem.SetTurnDoneByIndex(currentPartyMember);
            }
        }
    }

    public void SetCurrentlyAttackingMember(int i)
    {
        currentPartyMember = i;
    }

    public void SetToMinDamageMod()
    {
        damageMod = 0.1f;
    }
}
