using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    Title,
    Home,
    Story,
    Battle,
    Result,
    Gacha,
    Formation
}

/// <summary>
/// ゲーム全体を管理するメインコントローラー
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("ゲーム状態")]
    public GameState currentState = GameState.Battle; // 初期設定として定義


    [Header("キャラクター設定")]
    public List<PartyMember> partyMembers = new List<PartyMember>();
    public List<string> currentPartyFormation = new List<string>(); // 現在のパーティ編成リスト（最大4人、戦闘中変更不可）
    public Enemy enemy;

    [Header("UI参照")]
    public UIManager uiManager;
    public TypingController typingController;
    public SkillDatabase skillDatabase;

    [Header("フラグ管理")]
    public bool isGameOver = false;
    public bool isVictory = false;
    public bool isPaused = false;
    
    [Header("ストーリー参照")]
    public StoryData sampleStoryData; // 過去互換用
    public StoryData tutorialStory1; // 開始直後用
    public StoryData tutorialStory2; // 初回クリア時用

    // バフ状態管理
    public bool isBuffActive = false;
    public float buffTimer = 0f;
    public float buffDamageMultiplier = 2f;

    // スピードバフ（タイピング緩和）
    public bool isSpeedBuffActive = false;
    public float speedBuffTimer = 0f;

    // 次のターゲット表示用
    public int nextTargetIndex = -1;

    [Header("新スキル用状態管理")]
    public bool glassBarrierActive = false;
    public int glassReflectDamage = 0;
    public bool isSparkActive = false;
    public float sparkMultiplier = 1.5f;

    [Header("持続系スキル管理")]
    public float activeTurretTimer = 0f;
    public float activeRegenTimer = 0f;

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
        
        // チュートリアル進行状況の確認
        if (PlayerDataManager.Instance != null && !PlayerDataManager.Instance.HasSeenTutorialStory1 && tutorialStory1 != null)
        {
            // 初回起動時：ストーリー1を再生してからバトルへ
            currentState = GameState.Story;
            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.PlayStory(tutorialStory1, () => {
                    PlayerDataManager.Instance.MarkTutorialStory1Seen();
                    EndStoryTransitionToBattle();
                });
            }
            else
            {
                EndStoryTransitionToBattle();
            }
        }
        else
        {
            // チュートリアル1閲覧済み：直接ホーム画面からゲームを開始
            GoToHome();
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case GameState.Title:
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                break;
            case GameState.Home:
                uiManager?.ShowHomePanel();
                break;
            case GameState.Gacha:
                uiManager?.ShowGachaPanel();
                break;
        }
    }

    public void GoToBattle()
    {
        uiManager?.HideHomePanel();
        InitializeGame();
        EndStoryTransitionToBattle();
    }

    /// <summary>
    /// ホーム画面からガチャ画面へ遷移
    /// </summary>
    public void GoToGacha()
    {
        uiManager?.HideHomePanel();
        ChangeState(GameState.Gacha);
    }

    /// <summary>
    /// ホーム画面へ戻る
    /// </summary>
    public void GoToHome()
    {
        isGameOver = false;
        isVictory = false;
        ChangeState(GameState.Home);
    }

    // === 持続系・新スキル追加メソッド ===

    public void ActivateTurret()
    {
        activeTurretTimer = 10f;
        StartCoroutine(TurretCoroutine());
    }

    IEnumerator TurretCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < 10f)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
            if (enemy != null && !isGameOver && !isVictory)
            {
                enemy.TakeDamage(1); 
                Debug.Log("Turret deals 1 damage to enemy.");
            }
        }
    }

    public void ActivateRegen()
    {
        activeRegenTimer = 10f;
        StartCoroutine(RegenCoroutine());
    }

    IEnumerator RegenCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < 10f)
        {
            yield return new WaitForSeconds(2f);
            elapsed += 2f;
            if (!isGameOver && !isVictory)
            {
                foreach (var member in partyMembers)
                {
                    if (member.currentHP > 0) member.Heal(1);
                }
                Debug.Log("Regen heals party members by 1.");
            }
        }
    }

    public void ActivateGlass() { glassBarrierActive = true; glassReflectDamage = 1; StartCoroutine(DeactivateBuffAfterDelay(() => glassBarrierActive = false, 10f)); }
    public void ActivateSpark() { isSparkActive = true; StartCoroutine(DeactivateBuffAfterDelay(() => isSparkActive = false, 10f)); }
    public void ActivateTrick() { if (enemy != null) enemy.DetermineNextTarget(); }
    public void ActivateClock() { if (enemy != null) enemy.attackTimer = Mathf.Min(enemy.baseAttackInterval, enemy.attackTimer + 3f); }

    IEnumerator DeactivateBuffAfterDelay(System.Action onEnd, float delay)
    {
        yield return new WaitForSeconds(delay);
        onEnd?.Invoke();
    }

    void Update()
    {
        if (currentState != GameState.Battle) return; // バトル中以外はタイマーや勝敗判定を進めない
        if (isGameOver || isVictory) return;

        // バフタイマー管理
        UpdateBuffTimers();

        // 勝敗判定
        CheckGameEnd();
    }

    void InitializeGame()
    {
        // 味方初期化（HP 10）
        partyMembers.Clear();
        currentPartyFormation.Clear();

        // 保存された編成を使用
        List<string> unlocked = new List<string> { "GlassMan" }; // 初期フォールバック
        List<string> formation = new List<string> { "GlassMan" };
        
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.UnlockedCharacters.Count > 0)
        {
            unlocked = PlayerDataManager.Instance.UnlockedCharacters;
            formation = PlayerDataManager.Instance.PartyFormation;
            
            if (formation == null || formation.Count == 0)
            {
                formation = new List<string>(unlocked);
            }
        }

        int count = Mathf.Min(formation.Count, 4); // 最大4人まで
        for (int i = 0; i < count; i++)
        {
            string charName = formation[i];
            if (!string.IsNullOrEmpty(charName) && unlocked.Contains(charName))
            {
                partyMembers.Add(new PartyMember(charName, 10));
                currentPartyFormation.Add(charName);
            }
        }

        // 敵の初期化（HP 50）
        if (enemy != null)
        {
            enemy.Initialize(50, 2, 10f);
        }

        // UI更新（パーティ表示含む）
        if (uiManager != null)
        {
            uiManager.SetupPartyVisibility();
            uiManager.UpdateAllUI();
        }
    }

    void UpdateBuffTimers()
    {
        float deltaTime = Time.deltaTime;

        // 持続系スキルタイマーの更新
        if (activeTurretTimer > 0)
        {
            activeTurretTimer -= deltaTime;
            if (activeTurretTimer < 0) activeTurretTimer = 0;
        }

        if (activeRegenTimer > 0)
        {
            activeRegenTimer -= deltaTime;
            if (activeRegenTimer < 0) activeRegenTimer = 0;
        }

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

        // 各キャラの無敵時間とクールダウン更新
        foreach (var member in partyMembers)
        {
            member.UpdateInvincibility(Time.deltaTime);
            member.UpdateCooldown(Time.deltaTime);
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
        typingController?.DisableInput();

        if (PlayerDataManager.Instance != null && !PlayerDataManager.Instance.HasSeenTutorialStory2 && tutorialStory2 != null)
        {
            // 初回クリア時：ストーリー2を再生し、終了後にホーム画面へ
            currentState = GameState.Story;
            if (StoryManager.Instance != null)
            {
                StoryManager.Instance.PlayStory(tutorialStory2, () => {
                    int earnedCoins = 100;
                    PlayerDataManager.Instance.AddCoins(earnedCoins);
                    PlayerDataManager.Instance.MarkTutorialStory2Seen();
                    Debug.Log($"初回クリアボーナス。ハックコインを獲得しました: {earnedCoins}枚");
                    // 強制的にホーム画面へ
                    GoToHome();
                });
            }
            else
            {
                PlayerDataManager.Instance.AddCoins(100);
                PlayerDataManager.Instance.MarkTutorialStory2Seen();
                GoToHome();
            }
        }
        else
        {
            // 通常クリア時リザルト処理
            currentState = GameState.Result;
            int earnedCoins = 100;
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.AddCoins(earnedCoins);
                Debug.Log($"ハックコインを獲得しました: {earnedCoins}枚");
            }

            if (uiManager != null)
            {
                uiManager.ShowVictory(earnedCoins);
            }
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        uiManager?.ShowGameOver();
        typingController?.DisableInput();
    }

    public void EndStoryTransitionToBattle()
    {
        currentState = GameState.Battle;
        typingController?.EnableInput();

        // 初回バトル時の攻撃チュートリアル
        uiManager?.ShowAttackTutorial();
    }

    public void TogglePause()
    {
        if (currentState != GameState.Battle) return;
        if (isGameOver || isVictory) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            typingController?.DisableInput();
        }
        else
        {
            typingController?.EnableInput();
        }

        uiManager?.ToggleSkillDictionaryPanel(isPaused);
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

        // sparkが有効なら1.5倍にしてフラグ消費
        if (isSparkActive)
        {
            damage *= sparkMultiplier;
            isSparkActive = false; // 消費
            Debug.Log($"spark効果発動！ダメージが{sparkMultiplier}倍！");
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

    [Header("クールダウン関連")]
    public float currentCooldown = 0f;
    public float maxCooldown = 3.0f; // デフォルト3秒

    public PartyMember(string name, int maxHP)
    {
        this.name = name;
        this.maxHP = maxHP;
        this.currentHP = maxHP;
        this.currentCooldown = 0f;
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

    public void UpdateCooldown(float deltaTime)
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= deltaTime;
            if (currentCooldown < 0f)
            {
                currentCooldown = 0f;
            }
        }
    }

    public float GetHPRatio()
    {
        if (maxHP <= 0) return 0f;
        return (float)currentHP / maxHP;
    }
}
