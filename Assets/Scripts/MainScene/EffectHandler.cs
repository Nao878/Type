using System.Collections.Generic;
using UnityEngine;

// 入力された文字に応じた効果を処理する
public class EffectHandler : MonoBehaviour
{
    public CharacterManager characterManager;
    public SkillSettings skillSettings; // ScriptableObject参照

    public void ProcessWord(string word)
    {
        if (word.ToLower() == "apple") // 「apple」と入力されたら
        {
            Character player1 = characterManager.partyMembers.Count > 0 ? characterManager.partyMembers[0] : null;
            if (player1 != null && !player1.skillOnCooldown && player1.sp == player1.maxSp)
            {
                Character target = characterManager.GetLowestHPCharacter();
                if (target != null)
                {
                    target.Heal(skillSettings.appleHealAmount); // ScriptableObjectから取得
                    characterManager.UpdateAllHpUI();
                    player1.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
        else if (word.ToLower() == "poison") // 「poison」と入力されたら
        {
            Character player2 = characterManager.partyMembers.Count > 1 ? characterManager.partyMembers[1] : null;
            if (player2 != null && !player2.skillOnCooldown && player2.sp == player2.maxSp)
            {
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.ApplyPoison(skillSettings.poisonDamage, skillSettings.poisonInterval, skillSettings.poisonDuration);
                    player2.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
        else if (word.ToLower() == "stop") // 「stop」と入力されたら
        {
            Character player3 = characterManager.partyMembers.Count > 2 ? characterManager.partyMembers[2] : null;
            if (player3 != null && !player3.skillOnCooldown && player3.sp == player3.maxSp)
            {
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.AddDelay(skillSettings.stopDelayTime);
                    player3.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
        else if (word.ToLower() == "debuff") // 「debuff」と入力されたら
        {
            Character player4 = characterManager.partyMembers.Count > 3 ? characterManager.partyMembers[3] : null;
            if (player4 != null && !player4.skillOnCooldown && player4.sp == player4.maxSp)
            {
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.ApplyDebuff(skillSettings.debuffDuration, skillSettings.debuffDamageMultiplier);
                    player4.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
    }
}
