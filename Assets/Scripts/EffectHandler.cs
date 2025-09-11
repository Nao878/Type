using System.Collections.Generic;
using UnityEngine;

// ���͂��ꂽ�����ɉ��������ʂ���������
public class EffectHandler : MonoBehaviour
{
    public CharacterManager characterManager;

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
                    target.Heal(20); // ���̖�����HP��20��
                    characterManager.UpdateAllHpUI(); // �񕜌��HP UI�X�V
                    player1.StartSkillCooldown(); // �N�[���_�E���J�n��SP0�i���̃L�����ɂ͉e�����Ȃ��j
                    characterManager.UpdateAllSpUI(); // SP UI�X�V
                }
            }
        }
    }
}
