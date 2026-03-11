using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スキルを辞書型で管理し、単語に応じてスキルを発動
/// 新しいスキルを簡単に追加できる拡張性を確保
/// </summary>
public class SkillDatabase : MonoBehaviour
{
    [Header("参照")]
    public GameManager gameManager;
    public UIManager uiManager;
    public Enemy enemy;

    [Header("コンボ履歴")]
    private string lastSkillExecuted = "";
    private float lastSkillTime = 0f;

    private Dictionary<string, Action> skills;
    private Dictionary<string, string> skillDescriptions;
    public Dictionary<string, List<string>> characterSkills;

    void Awake()
    {
        InitializeSkills();
    }

    void Start()
    {
        // Start method added as per instruction, currently empty.
    }

    void InitializeSkills()
    {
        skills = new Dictionary<string, Action>()
        {
            // === 基本スキル4種 ===
            {"apple", SkillApple},      // 最もHPが低い味方を2回復
            {"stop", SkillStop},        // 10秒間、敵の攻撃タイマー進行速度0.5倍
            {"poison", SkillPoison},    // 敵に毒（10秒間、1秒ごとに1ダメージ）
            {"buff", SkillBuff},        // 10秒間、味方の全与ダメージ2倍

            // === 追加スキル15種 ===
            {"protect", SkillProtect},  // 5秒間、ランダムな味方1人を無敵化
            {"attack", SkillAttack},    // 敵に基本ダメージ3
            {"speed", SkillSpeed},      // 5秒間、タイピング判定緩和
            {"share", SkillShare},      // パーティ全員のHPを平均化
            {"erase", SkillErase},      // 敵の攻撃カウントダウンをリセット
            {"future", SkillFuture},    // 次に狙われる味方を強調表示
            {"change", SkillChange},    // 敵の属性/耐性をランダム変更
            {"reduce", SkillReduce},    // 敵の攻撃力を永続10%減少（重複不可）
            {"active", SkillActive},    // 全継続バフの効果時間を3秒延長
            {"believe", SkillBelieve},  // 30%の確率で発動中のダメージ3倍
            {"ignore", SkillIgnore},    // 敵の防御を無視してダメージ
            {"supply", SkillSupply},    // 味方全員のHPを1回復
            {"freeze", SkillFreeze},    // 3秒間、敵の攻撃タイマー完全停止
            {"divide", SkillDivide},    // 敵の現在HPの10%分のダメージ
            {"finish", SkillFinish},    // 敵HP10%以下（5以下）なら即座に勝利
            {"shield", SkillShield},    // 敵の大技準備中（3秒以内）に防御する
            {"water", SkillWater},      // 基本ダメージ1（コンボ用）

            // === 新規追加スキル ===
            {"cure", SkillCure},
            {"glass", SkillGlass},
            {"trick", SkillTrick},
            {"clock", SkillClock},
            {"scratch", SkillScratch},
            {"cat", SkillCat},
            {"fire", SkillFire},
            {"spark", SkillSpark}
        };

        skillDescriptions = new Dictionary<string, string>()
        {
            {"apple", "回復"},
            {"stop", "遅延"},
            {"poison", "毒"},
            {"buff", "攻撃強化"},
            {"protect", "無敵"},
            {"attack", "攻撃"},
            {"speed", "タイピング緩和"},
            {"share", "HP平均化"},
            {"erase", "攻撃リセット"},
            {"future", "ターゲット表示"},
            {"change", "ステータス変更"},
            {"reduce", "攻撃力低下"},
            {"active", "バフ延長"},
            {"believe", "確率3倍"},
            {"ignore", "固定ダメ"},
            {"supply", "全体1回復"},
            {"freeze", "停止"},
            {"divide", "割合ダメ"},
            {"finish", "即死"},
            {"shield", "防御"},
            {"water", "水撃"},

            {"cure", "デバフ解除"},
            {"glass", "反射バリア"},
            {"trick", "タゲ変更"},
            {"clock", "3秒巻戻し"},
            {"scratch", "2ダメージ"},
            {"cat", "1秒回避"},
            {"fire", "3ダメ+火傷"},
            {"spark", "次弾1.5倍"}
        };

        characterSkills = new Dictionary<string, List<string>>()
        {
            {"GlassMan", new List<string>{"apple", "supply", "protect", "cure", "glass"}},
            {"Gentleman", new List<string>{"stop", "freeze", "change", "trick", "clock"}},
            {"CatGirl", new List<string>{"poison", "speed", "scratch", "cat"}},
            {"YellowGirl", new List<string>{"attack", "believe", "finish", "fire", "spark"}}
        };
    }

    public List<string> GetAvailableSkills()
    {
        List<string> available = new List<string>();
        if (gameManager == null) return available;
        
        foreach (var member in gameManager.partyMembers)
        {
            // 生死に関わらず編成されていれば使えるか、生きている時だけか？
            // 仕様「プレイヤーは編成されているキャラクターのスペルのみ入力可能となります」に基づく
            // （現状は編成されていれば入力可能とするが、必要なら && member.currentHP > 0 を追加）
            if (characterSkills.ContainsKey(member.name))
            {
                available.AddRange(characterSkills[member.name]);
            }
        }
        return available;
    }

    public bool HasSkill(string word)
    {
        string key = word.ToLower();
        if (!skills.ContainsKey(key)) return false;
        
        // 利用可能なスキルリストに含まれるか判定
        var available = GetAvailableSkills();
        return available.Contains(key);
    }

    public List<string> GetSuggestions(string prefix, int maxCount = 3)
    {
        List<string> results = new List<string>();
        if (string.IsNullOrEmpty(prefix)) return results;

        string searchPrefix = prefix.ToLower();
        var available = GetAvailableSkills();

        foreach (var skillName in available)
        {
            if (skillName.StartsWith(searchPrefix) && skills.ContainsKey(skillName))
            {
                string desc = skillDescriptions.ContainsKey(skillName) ? skillDescriptions[skillName] : "??";
                results.Add($"{skillName}({desc})");
                if (results.Count >= maxCount) break;
            }
        }
        return results;
    }

    public Dictionary<string, string> GetAllSkillDescriptions()
    {
        Dictionary<string, string> allSkills = new Dictionary<string, string>();
        foreach (var kvp in skills)
        {
            string desc = skillDescriptions.ContainsKey(kvp.Key) ? skillDescriptions[kvp.Key] : "??";
            allSkills.Add(kvp.Key, desc);
        }
        return allSkills;
    }

    public void ActivateSkill(string skillWord)
    {
        string key = skillWord.ToLower();
        if (gameManager == null || gameManager.isGameOver || gameManager.isVictory) return;

        // --- コンボ履歴のチェック ---
        // 新しいスキルが発動するか、5秒経過すると履歴クリア
        if (Time.time - lastSkillTime > 5f)
        {
            lastSkillExecuted = "";
        }

        if (skills.ContainsKey(key))
        {
            skills[key].Invoke();
            uiManager?.ShowSkillActivation(key);
            Debug.Log($"スキル発動: {key}");

            // 直前に実行したスキルの履歴を更新
            lastSkillExecuted = key;
            lastSkillTime = Time.time;
        }
    }

    /// <summary>
    /// 新しいスキルを追加（拡張用）
    /// </summary>
    public void AddSkill(string word, Action skillAction)
    {
        if (!skills.ContainsKey(word.ToLower()))
        {
            skills.Add(word.ToLower(), skillAction);
        }
    }

    // ========================================
    // 基本スキル実装
    // ========================================

    void SkillApple()
    {
        // 最もHPが低い味方のHPを2回復
        PartyMember target = gameManager.GetLowestHPMember();
        if (target != null)
        {
            target.Heal(2);
            uiManager?.UpdateAllUI();
            Debug.Log($"apple: {target.name}を2回復");

            // protectappleコンボ
            if (lastSkillExecuted == "protect" && Time.time - lastSkillTime <= 5f)
            {
                target.SetInvincible(5f);
                uiManager?.ShowComboText("COMBO: PROTECTAPPLE!");
                uiManager?.ShowProtectEffect(gameManager.partyMembers.IndexOf(target));
                Debug.Log($"コンボ発動！ protect -> apple (protectapple: {target.name}を無敵化)");
            }
        }
    }

    void SkillStop()
    {
        // 10秒間、敵の攻撃タイマー進行速度0.5倍
        if (enemy != null)
        {
            enemy.ApplySlow(10f, 0.5f);
            Debug.Log("stop: 敵の攻撃速度を10秒間0.5倍に");
        }
    }

    void SkillPoison()
    {
        // 敵に毒（10秒間、1秒ごとに1ダメージ）
        if (enemy != null)
        {
            enemy.ApplyPoison(10f, 1);
            uiManager?.ShowPoisonEffect();
            Debug.Log("poison: 敵に毒を付与（10秒間、1ダメージ/秒）");
        }
    }

    void SkillBuff()
    {
        // 10秒間、味方の全与ダメージ2倍
        if (gameManager != null)
        {
            gameManager.isBuffActive = true;
            gameManager.buffTimer = 10f;
            gameManager.buffDamageMultiplier = 2f;
            Debug.Log("buff: 10秒間ダメージ2倍");
        }
    }

    // ========================================
    // 追加スキル実装
    // ========================================

    void SkillProtect()
    {
        // 5秒間、ランダムな味方1人を無敵化
        PartyMember target = gameManager.GetRandomAliveMember();
        if (target != null)
        {
            target.SetInvincible(5f);
            uiManager?.ShowProtectEffect(gameManager.partyMembers.IndexOf(target));
            Debug.Log($"protect: {target.name}を5秒間無敵化");
        }
    }

    void SkillAttack()
    {
        // 敵に基本ダメージ3
        if (enemy != null)
        {
            int damage = gameManager.CalculateDamage(3);
            enemy.TakeDamage(damage);
            uiManager?.UpdateEnemyHP();
            Debug.Log($"attack: 敵に{damage}ダメージ");
        }
    }

    void SkillSpeed()
    {
        // 5秒間、タイピング判定緩和
        if (gameManager != null)
        {
            gameManager.isSpeedBuffActive = true;
            gameManager.speedBuffTimer = 5f;
            Debug.Log("speed: 5秒間タイピング判定緩和");
        }
    }

    void SkillShare()
    {
        // パーティ全員のHPを平均化
        if (gameManager != null && gameManager.partyMembers.Count > 0)
        {
            int totalHP = 0;
            int aliveCount = 0;
            foreach (var m in gameManager.partyMembers)
            {
                if (m.currentHP > 0)
                {
                    totalHP += m.currentHP;
                    aliveCount++;
                }
            }

            if (aliveCount > 0)
            {
                int avgHP = totalHP / aliveCount;
                foreach (var m in gameManager.partyMembers)
                {
                    if (m.currentHP > 0)
                    {
                        m.currentHP = Mathf.Min(avgHP, m.maxHP);
                    }
                }
                uiManager?.UpdateAllUI();
                Debug.Log($"share: 全員のHPを{avgHP}に平均化");
            }
        }
    }

    void SkillErase()
    {
        // 敵の攻撃カウントダウンをリセット
        if (enemy != null)
        {
            enemy.ResetAttackTimer();
            Debug.Log("erase: 敵の攻撃タイマーをリセット");
        }
    }

    void SkillFuture()
    {
        // 次に狙われる味方を強調表示
        if (enemy != null && gameManager != null)
        {
            int targetIndex = enemy.nextTargetIndex;
            uiManager?.HighlightNextTarget(targetIndex);
            Debug.Log($"future: 次のターゲットはキャラ{targetIndex + 1}");
        }
    }

    void SkillChange()
    {
        // 敵の属性/耐性をランダムに変更（シンプル実装：ランダムバフ/デバフ）
        if (enemy != null)
        {
            // ランダムで何かの効果をリセットまたは変更
            float rand = UnityEngine.Random.value;
            if (rand < 0.5f && enemy.isAttackReduced)
            {
                enemy.isAttackReduced = false;
                Debug.Log("change: 敵の攻撃力減少がリセットされた");
            }
            else
            {
                enemy.baseDamage = UnityEngine.Random.Range(1, 4);
                Debug.Log($"change: 敵の基本ダメージが{enemy.baseDamage}に変更");
            }
        }
    }

    void SkillReduce()
    {
        // 敵の攻撃力を永続10%減少（重複不可）
        if (enemy != null)
        {
            enemy.ApplyAttackReduction();
            Debug.Log("reduce: 敵の攻撃力を永続10%減少");
        }
    }

    void SkillActive()
    {
        // 全継続バフの効果時間を3秒延長
        if (gameManager != null)
        {
            gameManager.ExtendAllBuffs(3f);
            Debug.Log("active: 全バフの効果時間を3秒延長");
        }
    }

    void SkillBelieve()
    {
        // 30%の確率で発動中のダメージ3倍（次の攻撃に適用）
        if (enemy != null)
        {
            int baseDamage = 3;
            int damage = gameManager.CalculateDamage(baseDamage, true);
            enemy.TakeDamage(damage);
            uiManager?.UpdateEnemyHP();

            // クリティカル判定（believe の3倍が発動したか）
            // buffなしの基本ダメージと比較して3倍以上ならクリティカル
            int normalDamage = gameManager.CalculateDamage(baseDamage, false);
            bool isCritical = damage >= normalDamage * 3;
            if (isCritical)
            {
                uiManager?.ShowCriticalEffect();
                Debug.Log($"believe: ★CRITICAL★ 敵に{damage}ダメージ！");
            }
            else
            {
                Debug.Log($"believe: 敵に{damage}ダメージ（30%で3倍）");
            }
        }
    }

    void SkillIgnore()
    {
        // 敵の防御を無視してダメージ（固定5ダメージ）
        if (enemy != null)
        {
            enemy.TakeDamage(5);
            uiManager?.UpdateEnemyHP();
            Debug.Log("ignore: 敵に防御無視5ダメージ");
        }
    }

    void SkillSupply()
    {
        // 味方全員のHPを1回復
        if (gameManager != null)
        {
            foreach (var m in gameManager.partyMembers)
            {
                if (m.currentHP > 0)
                {
                    m.Heal(1);
                }
            }
            uiManager?.UpdateAllUI();
            Debug.Log("supply: 味方全員を1回復");
        }
    }

    void SkillFreeze()
    {
        if (enemy != null)
        {
            float duration = 3f;

            // コンボ判定：直前(5秒以内)のスキルが "water" だった場合シナジー発生
            if (lastSkillExecuted == "water" && Time.time - lastSkillTime <= 5f)
            {
                duration = 6f; // コンボで2倍
                uiManager?.ShowComboText("COMBO: WATER -> FREEZE!");
                Debug.Log("コンボ発動！ water -> freeze (凍結時間が6秒に延長)");
            }
            // コンボ判定：直前が "poison" だった場合 (coldpoison)
            else if (lastSkillExecuted == "poison" && Time.time - lastSkillTime <= 5f)
            {
                duration = 6f; // コンボで2倍
                if (enemy.isPoisoned) enemy.poisonDamagePerTick *= 2; // 毒ダメージ倍増
                uiManager?.ShowComboText("COMBO: COLDPOISON!");
                Debug.Log("コンボ発動！ poison -> freeze (coldpoison: 凍結6秒＆毒ダメージ倍増)");
            }
            else
            {
                Debug.Log($"freeze 発動 (凍結時間3秒)");
            }

            enemy.ApplyFreeze(duration);
            uiManager?.ShowFreezeEffect();
        }
    }

    void SkillDivide()
    {
        // 敵の現在HPの10%分のダメージ
        if (enemy != null)
        {
            int damage = Mathf.Max(1, enemy.currentHP / 10);
            damage = gameManager.CalculateDamage(damage);
            enemy.TakeDamage(damage);
            uiManager?.UpdateEnemyHP();
            Debug.Log($"divide: 敵に{damage}ダメージ（現HP10%）");
        }
    }

    void SkillFinish()
    {
        // 敵HP10%以下（5以下）なら即座に勝利
        if (enemy != null && enemy.currentHP <= 5)
        {
            enemy.TakeDamage(enemy.currentHP);
            gameManager.Victory();
            Debug.Log("finish: 敵を撃破！勝利！");
        }
        else
        {
            Debug.Log("finish: 敵HPが10%より高いため発動失敗");
        }
    }

    void SkillShield()
    {
        // 敵の大技準備中（attackTimer <= 3f）に発動で防御成功
        if (enemy != null && enemy.isBigMoveQueued && enemy.attackTimer <= 3f && !enemy.isShieldActive)
        {
            enemy.isShieldActive = true;
            uiManager?.ShowWarningText(false);
            uiManager?.ShowBlockedEffect();
            Debug.Log("shield: 敵の大技ブロック準備完了！");
        }
        else
        {
            Debug.Log("shield: 大技のタイミングではないため不発");
        }
    }

    void SkillWater()
    {
        if (enemy != null)
        {
            int damage = 1;
            // 味方のダメージバフがアクティブなら2倍
            if (gameManager != null && gameManager.isBuffActive)
            {
                damage *= 2;
            }
            
            enemy.TakeDamage(damage);
            uiManager?.UpdateAllUI();
            uiManager?.ShowDamageEffect(-1); // 全体化エフェクトか無指定
            Debug.Log($"water 発動! 敵に {damage} ダメージ");
        }
    }

    // ========================================
    // 追加スキル（GlassMan, Gentleman, CatGirl, YellowGirl用）
    // ========================================

    void SkillCure()
    {
        // デバフ解除（今回は特定の状態がないため仮ログのみ）
        Debug.Log("cure: 味方のデバフを解除！");
    }

    void SkillGlass()
    {
        // 反射バリア
        if (gameManager != null)
        {
            gameManager.glassBarrierActive = true;
            Debug.Log("glass: 1回反射バリアを張った！");

            // fireglassコンボ
            if (lastSkillExecuted == "fire" && Time.time - lastSkillTime <= 5f)
            {
                gameManager.glassReflectDamage = 5; // 追加爆発ダメージ（仮で5に設定）
                uiManager?.ShowComboText("COMBO: FIREGLASS!");
                Debug.Log("コンボ発動！ fire -> glass (fireglass: 反射時爆発ダメージ＆火傷)");
            }
        }
    }

    void SkillTrick()
    {
        // ターゲット変更
        if (enemy != null)
        {
            // publicメソッド DetermineNextTarget() を追加して呼ぶか、インデックスを再抽選するか
            // ひとまず Enemy側に DetermineNextTarget をpublic化して対処（後ほどEnemy修正）
            enemy.DetermineNextTarget();
            Debug.Log("trick: 敵のターゲットシャッフル！");
        }
    }

    void SkillClock()
    {
        // タイマー3秒巻き戻し
        if (enemy != null)
        {
            enemy.attackTimer += 3f;
            Debug.Log("clock: 敵の攻撃タイマーを3秒巻き戻した！");
        }
    }

    void SkillScratch()
    {
        // 2ダメージ
        if (enemy != null)
        {
            int damage = 2;
            if (gameManager != null && gameManager.isBuffActive) damage *= 2;
            enemy.TakeDamage(damage);
            uiManager?.UpdateAllUI();
            Debug.Log($"scratch: 敵に {damage} ダメージ");
        }
    }

    void SkillCat()
    {
        // 1秒完全回避
        if (gameManager != null)
        {
            foreach (var member in gameManager.partyMembers)
            {
                if (member.currentHP > 0) member.SetInvincible(1f);
            }
            Debug.Log("cat: 1秒間全員完全回避！");
        }
    }

    void SkillFire()
    {
        // 3ダメ＋火傷
        if (enemy != null)
        {
            int damage = 3;
            if (gameManager != null && gameManager.isBuffActive) damage *= 2;
            enemy.TakeDamage(damage);
            enemy.ApplyBurn(10f, 1); // あとでEnemyに追加
            uiManager?.UpdateAllUI();
            Debug.Log($"fire: 敵に {damage} ダメージ ＋火傷付与");
        }
    }

    void SkillSpark()
    {
        // 次弾1.5倍
        if (gameManager != null)
        {
            gameManager.isSparkActive = true;
            Debug.Log("spark: 次の攻撃の威力1.5倍！");
        }
    }
}
