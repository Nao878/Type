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
    public GameManager gameManager;
    public Enemy enemy;
    public UIManager uiManager;

    private Dictionary<string, Action> skills;

    void Awake()
    {
        InitializeSkills();
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
            {"finish", SkillFinish}     // 敵HP10%以下（5以下）なら即座に勝利
        };
    }

    public bool HasSkill(string word)
    {
        return skills.ContainsKey(word.ToLower());
    }

    public void ActivateSkill(string word)
    {
        string key = word.ToLower();
        if (skills.ContainsKey(key))
        {
            skills[key].Invoke();
            uiManager?.ShowSkillActivation(key);
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
            Debug.Log($"believe: 敵に{damage}ダメージ（30%で3倍）");
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
        // 3秒間、敵の攻撃タイマー完全停止
        if (enemy != null)
        {
            enemy.ApplyFreeze(3f);
            uiManager?.ShowFreezeEffect();
            Debug.Log("freeze: 敵を3秒間フリーズ");
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
}
