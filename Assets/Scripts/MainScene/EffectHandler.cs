using System.Collections.Generic;
using UnityEngine;

// ���͂��ꂽ�����ɉ��������ʂ���������
public class EffectHandler : MonoBehaviour
{
    public CharacterManager characterManager;
    public SkillSettings skillSettings; // ScriptableObject�Q��

    public void ProcessWord(string word)
    {
        if (word.ToLower() == "apple") // �uapple�v�Ɠ��͂��ꂽ��
        {
            Character player1 = characterManager.partyMembers.Count > 0 ? characterManager.partyMembers[0] : null;
            if (player1 != null && !player1.skillOnCooldown && player1.sp == player1.maxSp)
            {
                Character target = characterManager.GetLowestHPCharacter();
                if (target != null)
                {
                    target.Heal(skillSettings.appleHealAmount); // ScriptableObject����擾
                    characterManager.UpdateAllHpUI();
                    player1.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
        else if (word.ToLower() == "poison") // �upoison�v�Ɠ��͂��ꂽ��
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
        else if (word.ToLower() == "stop") // �ustop�v�Ɠ��͂��ꂽ��
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
        else if (word.ToLower() == "debuff") // �udebuff�v�Ɠ��͂��ꂽ��
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
