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
            // �ł�HP�̒Ⴂ�����L������T��
            Character target = characterManager.GetLowestHPCharacter();

            if (target != null) // ���̖����L�����������珈��
            {
                target.Heal(20); // ���̖�����HP��20��
                characterManager.UpdateAllHpUI(); // �񕜌��UI�X�V
            }
        }
    }
}
