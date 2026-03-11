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

    [Header("火傷状態（新スキル）")]
    public bool isBurned = false;
    public float burnTimer = 0f;
    public float burnTickTimer = 0f;
    public int burnDamagePerTick = 1;

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

    [Header("発狂モード")]
    public bool isEnraged = false;
    private bool enrageTriggered = false;

    [Header("大技システム")]
    public bool isBigMoveQueued = false;
    public bool isShieldActive = false;
    private bool warningShown = false;

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
        GetNextAttackType();
        attackTimer = GetRandomAttackInterval();
        isEnraged = false;
        enrageTriggered = false;
        isInitialized = true;
    }

    void GetNextAttackType()
    {
        isBigMoveQueued = Random.value < 0.2f; // 20%の確率で大技
        warningShown = false;
        isShieldActive = false;
        gameManager?.uiManager?.ShowWarningText(false);
    }

    void Update()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.isGameOver || gameManager.isVictory)
        {
            gameManager?.uiManager?.ShowDangerText(false);
            gameManager?.uiManager?.ShowWarningText(false);
            return;
        }

        CheckEnrageMode();
        UpdateStatusEffects();
        UpdateAttackTimer();
    }

    void CheckEnrageMode()
    {
        if (!enrageTriggered && currentHP <= maxHP / 2)
        {
            enrageTriggered = true;
            isEnraged = true;
            baseAttackInterval /= 2f; // 攻撃間隔を半分に
            attackTimer /= 2f; // 現在のタイマーも半分に
            Debug.Log("Enemy: 発狂モード突入！攻撃間隔が半分になった！");
        }
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

        // 火傷ダメージ処理
        if (isBurned)
        {
            burnTimer -= Time.deltaTime;
            burnTickTimer += Time.deltaTime;

            if (burnTickTimer >= 1f)
            {
                int damage = gameManager.CalculateDamage(burnDamagePerTick);
                TakeDamage(damage);
                burnTickTimer -= 1f;
            }

            if (burnTimer <= 0f)
            {
                isBurned = false;
                burnTimer = 0f;
                burnTickTimer = 0f;
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

        // 大技の警告表示 (attackTimer <= 3.0f)
        if (isBigMoveQueued && attackTimer <= 3f && !warningShown)
        {
            warningShown = true;
            gameManager?.uiManager?.ShowWarningText(true);
        }

        // 発狂モード中のDANGER表示 (attackTimer <= 2.0f かつ 大技ではない時)
        if (isEnraged && !isBigMoveQueued)
        {
            if (attackTimer <= 2f)
            {
                gameManager?.uiManager?.ShowDangerText(true);
            }
            else
            {
                gameManager?.uiManager?.ShowDangerText(false);
            }
        }

        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = GetRandomAttackInterval();
            GetNextAttackType();
            DetermineNextTarget();
        }
    }

    void PerformAttack()
    {
        if (gameManager == null) return;

        // エフェクトを隠す
        gameManager.uiManager?.ShowWarningText(false);
        gameManager.uiManager?.ShowDangerText(false);

        PartyMember target = null;
        if (nextTargetIndex >= 0 && nextTargetIndex < gameManager.partyMembers.Count && gameManager.partyMembers[nextTargetIndex].currentHP > 0)
        {
            target = gameManager.partyMembers[nextTargetIndex];
        }
        else
        {
            target = gameManager.GetRandomAliveMember();
        }
        if (target == null) return;

        int damage = baseDamage;

        // 大技の処理
        if (isBigMoveQueued)
        {
            if (isShieldActive)
            {
                damage = 1; // 防御成功
                Debug.Log("Enemy: プレイヤーがシールドで大技を防御！");
                isShieldActive = false;
            }
            else
            {
                damage *= 3; // 防御失敗、3倍ダメージ
                Debug.Log("Enemy: 大技直撃！3倍ダメージ！");
            }
        }

        // 攻撃力減少適用
        if (isAttackReduced)
        {
            damage = Mathf.RoundToInt(damage * (1f - attackReductionPercent));
            damage = Mathf.Max(damage, 1); // 最低1ダメージ
        }

        // ガラスバリアによる反射処理
        if (gameManager.glassBarrierActive)
        {
            Debug.Log($"Enemy: 攻撃がガラスバリアで反射された！({damage}ダメージ)");
            TakeDamage(damage);
            
            // fireglassコンボの追加効果
            if (gameManager.glassReflectDamage > 0)
            {
                Debug.Log($"Enemy: fireglassコンボの追加爆発！({gameManager.glassReflectDamage}ダメージ＆火傷)");
                TakeDamage(gameManager.glassReflectDamage);
                ApplyBurn(10f, 1);
                gameManager.glassReflectDamage = 0;
            }

            gameManager.glassBarrierActive = false;
            gameManager.uiManager?.UpdateAllUI();
            return; // プレイヤーへのダメージ処理はスキップ
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

    public void DetermineNextTarget()
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
    /// 火傷を付与
    /// </summary>
    public void ApplyBurn(float duration, int damagePerTick)
    {
        isBurned = true;
        burnTimer = duration;
        burnTickTimer = 0f;
        burnDamagePerTick = damagePerTick;
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
        GetNextAttackType();
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
        if (isBurned) burnTimer += seconds;
        if (isSlowed) slowTimer += seconds;
        if (isFrozen) freezeTimer += seconds;
    }

    public float GetHPRatio()
    {
        if (maxHP <= 0) return 0f;
        return (float)currentHP / maxHP;
    }
}
