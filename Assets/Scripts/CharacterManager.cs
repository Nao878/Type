using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public List<Character> partyMembers = new List<Character>
    {
        new Character("��񂲔���̏���", 100, 100),
        new Character("�j�[�g", 80, 80),
        new Character("����ڂ�", 90, 90),
        new Character("����n�C���t���G���T�[", 70, 70),
    };

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

[System.Serializable]
public class Character
{
    public string name;
    public int hp;
    public int maxHp;

    public Character(string name, int hp, int maxHp)
    {
        this.name = name;
        this.hp = hp;
        this.maxHp = maxHp;
    }

    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);
        Debug.Log(name + " healed for " + amount + " HP. Current HP: " + hp);
    }

    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(hp - amount, 0);
        Debug.Log(name + " took " + amount + " damage! Current HP: " + hp);
    }
}
