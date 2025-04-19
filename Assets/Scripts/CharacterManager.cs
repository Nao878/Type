using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    // 味方達キャラ達のリスト
    public List<Character> partyMembers;

    void Awake()//本来は変数宣言で初期化するが、システム上0に上書きされたためAwakeで初期化する
    {
        if (partyMembers == null || partyMembers.Count == 0)
        {
            partyMembers = new List<Character>
            {
                new Character("りんご売りの少女", 100, 100),
                new Character("ニート", 80, 80),
                new Character("しょぼん", 90, 90),
                new Character("炎上系インフルエンサー", 70, 70),
            };
        }
    }

    // 全員がやられている(HPが0)かチェックする
    public bool IsAllDead()
    {
        foreach (var character in partyMembers)
        {
            if (character.hp > 0) return false;
        }
        return true;
    }

    // HPが最も低いキャラを取得する(死んだキャラは除外)
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

[System.Serializable]// この属性を付けることで、Unityのインスペクタで表示されるようになる
public class Character// MonoBehaviourがないので、オブジェクトにアタッチできない、newで作成できる、通常インスペクター表示されない、Start/Update使えない、軽量で部品向き
{
    public string name;
    public int hp;
    public int maxHp;

    // キャラを作る時に使うコンストラクタ
    public Character(string name, int hp, int maxHp)
    {
        this.name = name;
        this.hp = hp;
        this.maxHp = maxHp;
    }

    // キャラのHPを回復するメソッド
    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);// HPがmaxHpを超えないようにする
        Debug.Log(name + " healed for " + amount + " HP. Current HP: " + hp);
    }
    
    // キャラのHPを減らす(ダメージを受ける)メソッド
    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(hp - amount, 0);// HPが0未満にならないようにする
        Debug.Log(name + " took " + amount + " damage! Current HP: " + hp);
    }
}
