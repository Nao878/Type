using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine;

// �����L�����̊Ǘ��E����
public class CharacterManager : MonoBehaviour
{
    // �����B�L�����B�̃��X�g
    public List<Character> partyMembers;

    public List<HpUIController> hpUIControllers; // HP�o�[UI���X�g�ƈ�v���Ă���K�v����

    // �S��������Ă���(HP��0)���`�F�b�N����
    public bool IsAllDead()
    {
        foreach (var character in partyMembers)
        {
            if (character.hp > 0) return false;
        }
        return true;
    }

    // HP���ł��Ⴂ�L�������擾����(���񂾃L�����͏��O)
    public Character GetLowestHPCharacter()
    {
        Character lowestHPChar = null;
        foreach (var character in partyMembers)
        {
            if (lowestHPChar == null || character.hp < lowestHPChar.hp)
            {
                lowestHPChar = character;
            }
        }
        return lowestHPChar;
    }

    // HP�o�[UI���X�V����
    public void UpdateAllHpUI()
    {
        for (int i = 0; i < partyMembers.Count; i++)
        {
            hpUIControllers[i].UpdateHpBar(partyMembers[i].GetHpRatio());
        }
    }

    // �������S�ł��Ă��邩�`�F�b�N
    public Character GetRandomAlly()
    {
        List<Character> alive = partyMembers.FindAll(c => c.hp > 0);
        if (alive.Count == 0) return null;
        return alive[Random.Range(0, alive.Count)];
    }
}


//�����L�����N�^�[�̏���ێ�����N���X
[System.Serializable]// ���̑�����t���邱�ƂŁAUnity�̃C���X�y�N�^�ŕ\�������悤�ɂȂ�
public class Character// MonoBehaviour���Ȃ��̂ŁA�I�u�W�F�N�g�ɃA�^�b�`�ł��Ȃ��Anew�ō쐬�ł���A�ʏ�C���X�y�N�^�[�\������Ȃ��AStart/Update�g���Ȃ��A�y�ʂŕ��i����
{
    public string name;
    public int hp;
    public int maxHp;

    // �L��������鎞�Ɏg���R���X�g���N�^
    public Character(string name, int hp, int maxHp)
    {
        this.name = name;
        this.hp = hp;
        this.maxHp = maxHp;
    }

    // �L������HP���񕜂��郁�\�b�h
    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);// HP��maxHp�𒴂��Ȃ��悤�ɂ���
        Debug.Log(name + " healed for " + amount + " HP. Current HP: " + hp);
    }
    
    // �L������HP�����炷(�_���[�W���󂯂�)���\�b�h
    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(hp - amount, 0);// HP��0�����ɂȂ�Ȃ��悤�ɂ���
        Debug.Log(name + " took " + amount + " damage! Current HP: " + hp);
    }

    //HP�̊���(0.0�`1.0)��Ԃ�
    public float GetHpRatio()
    {
        return (float)hp / maxHp;
    }
}
