using System.Collections.Generic;
using UnityEngine;

// 入力された文字に応じた効果を処理する
public class EffectHandler : MonoBehaviour
{
    public CharacterManager characterManager;

    [Header("Apple Skill Settings")]
    public int appleHealAmount = 20;

    [Header("Poison Skill Settings")]
    public int poisonDamage = 1;
    public float poisonInterval = 1f;
    public float poisonDuration = 5f;

    [Header("Stop Skill Settings")]
    public float stopDelayTime = 2f;

    [Header("Debuff Skill Settings")]
    public float debuffDuration = 3f;
    public int debuffDamageMultiplier = 2;

    public void ProcessWord(string word)
    {
        if (word.ToLower() == "apple") // 「apple」と入力されたら
        {
            // プレイヤー1（partyMembers[0]）のスキルとして回復
            Character player1 = characterManager.partyMembers.Count > 0 ? characterManager.partyMembers[0] : null;
            if (player1 != null && !player1.skillOnCooldown && player1.sp == player1.maxSp)
            {
                Character target = characterManager.GetLowestHPCharacter();
                if (target != null)
                {
                    target.Heal(appleHealAmount); // その味方のHPを回復量で回復
                    characterManager.UpdateAllHpUI(); // 回復後にHP UI更新
                    player1.StartSkillCooldown(); // クールダウン開始＆SP0（他のキャラには影響しない）
                    characterManager.UpdateAllSpUI(); // SP UI更新
                }
            }
        }
        else if (word.ToLower() == "poison") // 「poison」と入力されたら
        {
            // プレイヤー2（partyMembers[1]）のスキル
            Character player2 = characterManager.partyMembers.Count > 1 ? characterManager.partyMembers[1] : null;
            if (player2 != null && !player2.skillOnCooldown && player2.sp == player2.maxSp)
            {
                // 敵に毒効果を付与
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.ApplyPoison(poisonDamage, poisonInterval, poisonDuration);
                    player2.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
        else if (word.ToLower() == "stop") // 「stop」と入力されたら
        {
            // プレイヤー3（partyMembers[2]）のスキル
            Character player3 = characterManager.partyMembers.Count > 2 ? characterManager.partyMembers[2] : null;
            if (player3 != null && !player3.skillOnCooldown && player3.sp == player3.maxSp)
            {
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.AddDelay(stopDelayTime); // 2秒遅延を加算
                    player3.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
        else if (word.ToLower() == "debuff") // 「debuff」と入力されたら
        {
            // プレイヤー4（partyMembers[3]）のスキル
            Character player4 = characterManager.partyMembers.Count > 3 ? characterManager.partyMembers[3] : null;
            if (player4 != null && !player4.skillOnCooldown && player4.sp == player4.maxSp)
            {
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.ApplyDebuff(debuffDuration, debuffDamageMultiplier); // 3秒間デバフ
                    player4.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
    }
}
