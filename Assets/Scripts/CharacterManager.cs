using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    // �����B�L�����B�̃��X�g
    public List<Character> partyMembers;

    void Awake()//�{���͕ϐ��錾�ŏ��������邪�A�V�X�e����0�ɏ㏑�����ꂽ����Awake�ŏ���������
    {
        if (partyMembers == null || partyMembers.Count == 0)
        {
            partyMembers = new List<Character>
            {
                new Character("��񂲔���̏���", 100, 100),
                new Character("�j�[�g", 80, 80),
                new Character("����ڂ�", 90, 90),
                new Character("����n�C���t���G���T�[", 70, 70),
            };
        }
    }

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

    public Character GetRandomAlly()
    {
        List<Character> alive = partyMembers.FindAll(c => c.hp > 0);
        if (alive.Count == 0) return null;
        return alive[Random.Range(0, alive.Count)];
    }
}

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
}
