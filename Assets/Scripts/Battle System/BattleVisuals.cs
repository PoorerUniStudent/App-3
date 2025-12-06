using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleVisuals : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI levelText;

    private int currentHealth;
    private int maxHealth;
    private int level;

    private Animator anim;

    private const string LVL_ABBR = "Lv. ";

    private const string IS_ATTACK_PARAM = "isAttack";
    private const string IS_HIT_PARAM = "isHit";
    private const string IS_DEAD_PARAM = "isDead";

    // Player only
    private const string IS_SHOOT_PARAM = "isShoot";
    private const string IS_ATTACK_RELEASE_PARAM = "isAttackRelease";

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetStartingValues(int currentHealth, int maxHealth, int level)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        this.level = level;

        levelText.text = LVL_ABBR + this.level.ToString();
        UpdateHealthBar();
    }

    public void ChangeHealth(int currentHealth)
    {
        this.currentHealth = currentHealth;
        // if our health == 0, play dead animation and destroy battle visual
        if (currentHealth <= 0)
        {
            PlayDeadAnimation();
            Destroy(gameObject, 1f);
        }

        UpdateHealthBar();
    }

    public void ChangeLevel(int level)
    {
        this.level = level;
        levelText.text = LVL_ABBR + this.level.ToString();
    }

    public void UpdateHealthBar()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    public void PlayAttackAnimation()
    {
        anim.SetTrigger(IS_ATTACK_PARAM);
    }

    public void PlayHitAnimation()
    {
        anim.SetTrigger(IS_HIT_PARAM);
    }

    public void PlayDeadAnimation()
    {
        anim.SetTrigger(IS_DEAD_PARAM);
    }

    public void PlayShootAnimation()
    {
        anim.SetTrigger(IS_SHOOT_PARAM);
    }

    public void PlayAttackReleaseAnimation()
    {
        anim.SetTrigger(IS_ATTACK_RELEASE_PARAM);
    }
}
