using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("大技システム")]
    public bool isBigMoveQueued = false;
    public bool isShieldActive = false;

    [Header("参照")]
    public GameManager gameManager;

    [Header("ユニットビジュアル")]
    public Sprite enemyUnitSprite;

    private bool isInitialized = false;

    void Start()
    {
#if UNITY_EDITOR
        if (enemyUnitSprite == null)
        {
            enemyUnitSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/EnemyEntity.png");
        }
#endif
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
        isEnraged = false;
        isInitialized = true;
        StartCoroutine(SpawnUnitRoutine());
    }

    void UpdateStatusEffects()
    {
        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0) isFrozen = false;
            return;
        }

        if (isPoisoned)
        {
            poisonTimer -= Time.deltaTime;
            poisonTickTimer += Time.deltaTime;
            if (poisonTickTimer >= 1.0f)
            {
                TakeDamage(poisonDamagePerTick);
                poisonTickTimer = 0f;
            }
            if (poisonTimer <= 0) isPoisoned = false;
        }

        if (isBurned)
        {
            burnTimer -= Time.deltaTime;
            burnTickTimer += Time.deltaTime;
            if (burnTickTimer >= 1.0f)
            {
                TakeDamage(burnDamagePerTick);
                burnTickTimer = 0f;
            }
            if (burnTimer <= 0) isBurned = false;
        }

        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0) isSlowed = false;
        }
    }

    void Update()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.isGameOver || gameManager.isVictory) return;

        // バトル中以外は停止
        if (gameManager.currentState != GameState.Battle) return;

        UpdateStatusEffects();
        // 直接攻撃（UpdateAttackTimer）は廃止し、SpawnUnitRoutine（コルーチン）に集約
    }

    IEnumerator SpawnUnitRoutine()
    {
        // 戦闘間隔を 8-12秒に広げる
        while (currentHP > 0)
        {
            float interval = Random.Range(8.0f, 12.0f);
            yield return new WaitForSeconds(interval);

            if (gameManager != null && gameManager.currentState == GameState.Battle && !gameManager.isGameOver && !gameManager.isVictory)
            {
                SpawnEnemyUnit();
            }
        }
    }

    void SpawnEnemyUnit()
    {
        // 召喚用の親オブジェクトを決定 (Canvas 直下に統一して座標ズレを防ぐ)
        Transform targetParent = null;
        if (GameManager.Instance != null && GameManager.Instance.spawnPoint != null)
        {
            targetParent = GameManager.Instance.spawnPoint.parent; // Canvas を想定
        }

        if (targetParent == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null) targetParent = canvas.transform;
        }

        if (targetParent == null)
        {
            Debug.LogError("No Canvas found to spawn EnemyUnit!");
            return;
        }

        // 自身の RectTransform チェック（座標計算のため）
        RectTransform bossRect = GetComponent<RectTransform>();
        Vector2 spawnBasePos = Vector2.zero;
        if (bossRect != null)
        {
            spawnBasePos = bossRect.anchoredPosition;
        }
        else
        {
            // RectTransform がない場合、デフォルトの右端付近（味方と同じ高さ Y=-200）に設定
            spawnBasePos = new Vector2(750, -200);
        }

        GameObject unitObj = new GameObject("EnemyUnit_Minion");
        unitObj.transform.SetParent(targetParent, false); // オフセットのある EnemyArea ではなく Canvas 等に配置

        RectTransform rect = unitObj.AddComponent<RectTransform>();
        unitObj.transform.localScale = Vector3.one; // スケールを確実に1に固定
        // ボスの現在位置（またはデフォルト設定）から少し左に出現させる
        rect.anchoredPosition = spawnBasePos + new Vector2(-50, 0);
        rect.sizeDelta = new Vector2(100, 100);
        
        // 見た目（Canvas上で表示するため Image を使用）
        Image img = unitObj.AddComponent<Image>();
        if (enemyUnitSprite != null)
        {
            img.sprite = enemyUnitSprite;
        }
        img.color = Color.white; // 元の画像の色を出すため白

        // 挙動
        EnemyUnit script = unitObj.AddComponent<EnemyUnit>();
        script.moveSpeed = 80f;
        script.hp = 10;
        script.damage = 2;
        script.attackInterval = 1.5f;

        Debug.Log($"Enemy Boss spawned a minion at {rect.anchoredPosition} under {targetParent.name}");

        // BattleManagerに登録
        if (BattleManager.Instance != null) BattleManager.Instance.RegisterEnemy(script);
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
