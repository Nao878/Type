using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ゲーム全体を管理するメインコントローラー
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("キャラクター設定")]
    public List<PartyMember> partyMembers = new List<PartyMember>();
    public Enemy enemy;

    [Header("UI参照")]
    public UIManager uiManager;
    public TypingController typingController;
    public SkillDatabase skillDatabase;

    [Header("ゲーム状態")]
    public bool isGameOver = false;
    public bool isVictory = false;

    // バフ状態管理
    public bool isBuffActive = false;
    public float buffTimer = 0f;
    public float buffDamageMultiplier = 2f;

    // スピードバフ（タイピング緩和）
    public bool isSpeedBuffActive = false;
    public float speedBuffTimer = 0f;

    // 次のターゲット表示用
    public int nextTargetIndex = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (isGameOver || isVictory) return;

        // バフタイマー管理
        UpdateBuffTimers();

        // 勝敗判定
        CheckGameEnd();
    }

    void InitializeGame()
    {
        // 味方4人の初期化（HP 10）
        partyMembers.Clear();
        partyMembers.Add(new PartyMember("キャラ1", 10));
        partyMembers.Add(new PartyMember("キャラ2", 10));
        partyMembers.Add(new PartyMember("キャラ3", 10));
        partyMembers.Add(new PartyMember("キャラ4", 10));

        // 敵の初期化（HP 50）
        if (enemy != null)
        {
            enemy.Initialize(50, 2, 10f);
        }

        // UI更新
        if (uiManager != null)
        {
            uiManager.UpdateAllUI();
        }
    }

    void UpdateBuffTimers()
    {
        // 攻撃バフ
        if (isBuffActive)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                isBuffActive = false;
                buffTimer = 0f;
            }
            uiManager?.UpdateBuffDisplay("buff", buffTimer);
        }

        // スピードバフ
        if (isSpeedBuffActive)
        {
            speedBuffTimer -= Time.deltaTime;
            if (speedBuffTimer <= 0f)
            {
                isSpeedBuffActive = false;
                speedBuffTimer = 0f;
            }
            uiManager?.UpdateBuffDisplay("speed", speedBuffTimer);
        }

        // 各キャラの無敵時間更新
        foreach (var member in partyMembers)
        {
            member.UpdateInvincibility(Time.deltaTime);
        }
    }

    void CheckGameEnd()
    {
        // 敵HP 0で勝利
        if (enemy != null && enemy.currentHP <= 0)
        {
            Victory();
            return;
        }

        // 味方全滅でゲームオーバー
        bool allDead = true;
        foreach (var member in partyMembers)
        {
            if (member.currentHP > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            GameOver();
        }
    }

    public void Victory()
    {
        isVictory = true;
        uiManager?.ShowVictory();
        typingController?.DisableInput();
    }

    public void GameOver()
    {
        isGameOver = true;
        uiManager?.ShowGameOver();
        typingController?.DisableInput();
    }

    /// <summary>
    /// ダメージ計算（buff適用）
    /// </summary>
    public int CalculateDamage(int baseDamage, bool applyBelieve = false)
    {
        float damage = baseDamage;

        // buffが有効なら2倍
        if (isBuffActive)
        {
            damage *= buffDamageMultiplier;
        }

        // believeが有効なら30%で3倍
        if (applyBelieve && UnityEngine.Random.value < 0.3f)
        {
            damage *= 3f;
        }

        return Mathf.RoundToInt(damage);
    }

    /// <summary>
    /// 最もHPが低い味方を取得
    /// </summary>
    public PartyMember GetLowestHPMember()
    {
        PartyMember lowest = null;
        foreach (var member in partyMembers)
        {
            if (member.currentHP > 0)
            {
                if (lowest == null || member.currentHP < lowest.currentHP)
                {
                    lowest = member;
                }
            }
        }
        return lowest;
    }

    /// <summary>
    /// ランダムな生存味方を取得
    /// </summary>
    public PartyMember GetRandomAliveMember()
    {
        List<PartyMember> alive = partyMembers.FindAll(m => m.currentHP > 0);
        if (alive.Count == 0) return null;
        return alive[UnityEngine.Random.Range(0, alive.Count)];
    }

    /// <summary>
    /// 全バフの効果時間延長
    /// </summary>
    public void ExtendAllBuffs(float seconds)
    {
        if (isBuffActive) buffTimer += seconds;
        if (isSpeedBuffActive) speedBuffTimer += seconds;

        foreach (var member in partyMembers)
        {
            if (member.isInvincible)
            {
                member.invincibilityTimer += seconds;
            }
        }

        if (enemy != null)
        {
            enemy.ExtendDebuffs(seconds);
        }
    }
}

/// <summary>
/// 味方キャラクターのデータクラス
/// </summary>
[System.Serializable]
public class PartyMember
{
    public string name;
    public int currentHP;
    public int maxHP;
    public bool isInvincible = false;
    public float invincibilityTimer = 0f;

    public PartyMember(string name, int maxHP)
    {
        this.name = name;
        this.maxHP = maxHP;
        this.currentHP = maxHP;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;
        currentHP = Mathf.Max(currentHP - amount, 0);
    }

    public void SetInvincible(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
    }

    public void UpdateInvincibility(float deltaTime)
    {
        if (isInvincible)
        {
            invincibilityTimer -= deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                invincibilityTimer = 0f;
            }
        }
    }

    public float GetHPRatio()
    {
        if (maxHP <= 0) return 0f;
        return (float)currentHP / maxHP;
    }
}
