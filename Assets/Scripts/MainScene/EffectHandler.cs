using System.Collections.Generic;
using UnityEngine;

// ���͂��ꂽ�����ɉ��������ʂ���������
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
        if (word.ToLower() == "apple") // �uapple�v�Ɠ��͂��ꂽ��
        {
            // �v���C���[1�ipartyMembers[0]�j�̃X�L���Ƃ��ĉ�
            Character player1 = characterManager.partyMembers.Count > 0 ? characterManager.partyMembers[0] : null;
            if (player1 != null && !player1.skillOnCooldown && player1.sp == player1.maxSp)
            {
                Character target = characterManager.GetLowestHPCharacter();
                if (target != null)
                {
                    target.Heal(appleHealAmount); // ���̖�����HP���񕜗ʂŉ�
                    characterManager.UpdateAllHpUI(); // �񕜌��HP UI�X�V
                    player1.StartSkillCooldown(); // �N�[���_�E���J�n��SP0�i���̃L�����ɂ͉e�����Ȃ��j
                    characterManager.UpdateAllSpUI(); // SP UI�X�V
                }
            }
        }
        else if (word.ToLower() == "poison") // �upoison�v�Ɠ��͂��ꂽ��
        {
            // �v���C���[2�ipartyMembers[1]�j�̃X�L��
            Character player2 = characterManager.partyMembers.Count > 1 ? characterManager.partyMembers[1] : null;
            if (player2 != null && !player2.skillOnCooldown && player2.sp == player2.maxSp)
            {
                // �G�ɓŌ��ʂ�t�^
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.ApplyPoison(poisonDamage, poisonInterval, poisonDuration);
                    player2.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
        else if (word.ToLower() == "stop") // �ustop�v�Ɠ��͂��ꂽ��
        {
            // �v���C���[3�ipartyMembers[2]�j�̃X�L��
            Character player3 = characterManager.partyMembers.Count > 2 ? characterManager.partyMembers[2] : null;
            if (player3 != null && !player3.skillOnCooldown && player3.sp == player3.maxSp)
            {
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.AddDelay(stopDelayTime); // 2�b�x�������Z
                    player3.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
        else if (word.ToLower() == "debuff") // �udebuff�v�Ɠ��͂��ꂽ��
        {
            // �v���C���[4�ipartyMembers[3]�j�̃X�L��
            Character player4 = characterManager.partyMembers.Count > 3 ? characterManager.partyMembers[3] : null;
            if (player4 != null && !player4.skillOnCooldown && player4.sp == player4.maxSp)
            {
                EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.ApplyDebuff(debuffDuration, debuffDamageMultiplier); // 3�b�ԃf�o�t
                    player4.StartSkillCooldown();
                    characterManager.UpdateAllSpUI();
                }
            }
        }
    }
}
