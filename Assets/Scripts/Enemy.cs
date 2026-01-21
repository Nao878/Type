using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵キャラクター（ロボット）の管理
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("基本ステータス")]
    public int currentHP = 50;
    public int maxHP = 50;
    public int baseDamage = 2;
    public float baseAttackInterval = 10f;

    [Header("状態異常")]
    // 毒状態
    public bool isPoisoned = false;
    public float poisonTimer = 0f;
    public float poisonTickTimer = 0f;
    public int poisonDamagePerTick = 1;

    // スロー状態（attackタイマー進行速度低下）
    public bool isSlowed = false;
    public float slowTimer = 0f;
    public float slowMultiplier = 0.5f;

    // フリーズ状態（攻撃完全停止）
    public bool isFrozen = false;
    public float freezeTimer = 0f;

    // 攻撃力減少（永続、重複不可）
    public bool isAttackReduced = false;
    public float attackReductionPercent = 0.1f;

    // 防御無視状態
    public bool ignoreDefenseActive = false;

    [Header("攻撃タイマー")]
    public float attackTimer = 0f;
    public int nextTargetIndex = -1;

    [Header("参照")]
    public GameManager gameManager;

    private bool isInitialized = false;

    void Start()
    {
        if (!isInitialized)
        {
            Initialize(50, 2, 10f);
        }
        DetermineNextTarget();
    }

    public void Initialize(int hp, int damage, float interval)
    {
        maxHP = hp;
        currentHP = hp;
        baseDamage = damage;
        baseAttackInterval = interval;
        attackTimer = GetRandomAttackInterval();
        isInitialized = true;
    }

    void Update()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.isGameOver || gameManager.isVictory) return;

        UpdateStatusEffects();
        UpdateAttackTimer();
    }

    void UpdateStatusEffects()
    {
        // 毒ダメージ処理
        if (isPoisoned)
        {
            poisonTimer -= Time.deltaTime;
            poisonTickTimer += Time.deltaTime;

            if (poisonTickTimer >= 1f)
            {
                int damage = gameManager.CalculateDamage(poisonDamagePerTick);
                TakeDamage(damage);
                poisonTickTimer -= 1f;
            }

            if (poisonTimer <= 0f)
            {
                isPoisoned = false;
                poisonTimer = 0f;
                poisonTickTimer = 0f;
            }
        }

        // スロー効果終了判定
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f)
            {
                isSlowed = false;
                slowTimer = 0f;
            }
        }

        // フリーズ終了判定
        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0f)
            {
                isFrozen = false;
                freezeTimer = 0f;
            }
        }
    }

    void UpdateAttackTimer()
    {
        // フリーズ中は攻撃しない
        if (isFrozen) return;

        // タイマー進行（スロー時は0.5倍速）
        float timerSpeed = isSlowed ? slowMultiplier : 1f;
        attackTimer -= Time.deltaTime * timerSpeed;

        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = GetRandomAttackInterval();
            DetermineNextTarget();
        }
    }

    void PerformAttack()
    {
        if (gameManager == null) return;

        PartyMember target = gameManager.GetRandomAliveMember();
        if (target == null) return;

        // ダメージ計算（攻撃力減少適用）
        int damage = baseDamage;
        if (isAttackReduced)
        {
            damage = Mathf.RoundToInt(damage * (1f - attackReductionPercent));
            damage = Mathf.Max(damage, 1); // 最低1ダメージ
        }

        target.TakeDamage(damage);
        gameManager.uiManager?.UpdateAllUI();
        gameManager.uiManager?.ShowDamageEffect(gameManager.partyMembers.IndexOf(target));
    }

    float GetRandomAttackInterval()
    {
        // 平均10秒のランダム間隔（8〜12秒）
        return Random.Range(baseAttackInterval * 0.8f, baseAttackInterval * 1.2f);
    }

    void DetermineNextTarget()
    {
        if (gameManager == null) return;

        List<int> aliveIndices = new List<int>();
        for (int i = 0; i < gameManager.partyMembers.Count; i++)
        {
            if (gameManager.partyMembers[i].currentHP > 0)
            {
                aliveIndices.Add(i);
            }
        }

        if (aliveIndices.Count > 0)
        {
            nextTargetIndex = aliveIndices[Random.Range(0, aliveIndices.Count)];
            gameManager.nextTargetIndex = nextTargetIndex;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(currentHP - amount, 0);
        gameManager?.uiManager?.UpdateEnemyHP();
    }

    /// <summary>
    /// 毒を付与
    /// </summary>
    public void ApplyPoison(float duration, int damagePerTick)
    {
        isPoisoned = true;
        poisonTimer = duration;
        poisonTickTimer = 0f;
        poisonDamagePerTick = damagePerTick;
    }

    /// <summary>
    /// スロー効果を付与
    /// </summary>
    public void ApplySlow(float duration, float multiplier)
    {
        isSlowed = true;
        slowTimer = duration;
        slowMultiplier = multiplier;
    }

    /// <summary>
    /// フリーズを付与
    /// </summary>
    public void ApplyFreeze(float duration)
    {
        isFrozen = true;
        freezeTimer = duration;
    }

    /// <summary>
    /// 攻撃タイマーをリセット
    /// </summary>
    public void ResetAttackTimer()
    {
        attackTimer = GetRandomAttackInterval();
    }

    /// <summary>
    /// 攻撃力を永続的に減少（重複不可）
    /// </summary>
    public void ApplyAttackReduction()
    {
        if (!isAttackReduced)
        {
            isAttackReduced = true;
        }
    }

    /// <summary>
    /// 全デバフの効果時間を延長
    /// </summary>
    public void ExtendDebuffs(float seconds)
    {
        if (isPoisoned) poisonTimer += seconds;
        if (isSlowed) slowTimer += seconds;
        if (isFrozen) freezeTimer += seconds;
    }

    public float GetHPRatio()
    {
        if (maxHP <= 0) return 0f;
        return (float)currentHP / maxHP;
    }
}
