using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine;

// 味方キャラの管理・検索
public class CharacterManager : MonoBehaviour
{
    // ScriptableObjectでキャラクターデータ管理
    public List<CharacterData> characterDataList; // 新規追加
    public List<Character> partyMembers;
    public List<HpUIController> hpUIControllers; // HPバーUIリストと一致している必要あり
    public List<SpUIController> spUIControllers; // SPバーUIリスト（partyMembersと一致）
    public GameObject gameOverObj; // 全滅時にアクティブにするオブジェクト
    public List<CharacterVisual> characterVisuals; // 各キャラの見た目制御

    void Awake()
    {
        // ScriptableObjectからpartyMembersを初期化
        if (characterDataList != null && characterDataList.Count > 0)
        {
            partyMembers = new List<Character>();
            foreach (var data in characterDataList)
            {
                partyMembers.Add(new Character(data.characterName, data.maxHp, data.maxHp) { maxSp = data.maxSp });
            }
        }
    }

    void Update()
    {
        foreach (var character in partyMembers)
        {
            character.UpdateCooldown(Time.deltaTime);
        }
        UpdateAllSpUI();

        // 全滅判定
        if (IsAllDead() && gameOverObj != null)
        {
            gameOverObj.SetActive(true);
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

    // HPバーUIを更新する
    public void UpdateAllHpUI()
    {
        int count = Mathf.Min(partyMembers.Count, hpUIControllers.Count);
        for (int i = 0; i < count; i++)
        {
            hpUIControllers[i].UpdateHpBar(partyMembers[i].GetHpRatio());
            // HP0なら暗く、そうでなければ元の色
            if (characterVisuals != null && characterVisuals.Count > i)
            {
                if (partyMembers[i].hp == 0)
                    characterVisuals[i].SetDeadVisual();
                else
                    characterVisuals[i].SetAliveVisual();
            }
        }
    }

    // SPバーUIを更新する
    public void UpdateAllSpUI()
    {
        int count = Mathf.Min(partyMembers.Count, spUIControllers.Count);
        for (int i = 0; i < count; i++)
        {
            spUIControllers[i].UpdateSpBar(partyMembers[i].GetSpRatio());
        }
    }

    // 味方が全滅しているかチェック
    public Character GetRandomAlly()
    {
        List<Character> alive = partyMembers.FindAll(c => c.hp > 0);
        if (alive.Count == 0) return null;
        return alive[Random.Range(0, alive.Count)];
    }
}


//味方キャラクターの情報を保持するクラス
[System.Serializable]// この属性を付けることで、Unityのインスペクタで表示されるようになる
public class Character// MonoBehaviourがないので、オブジェクトにアタッチできない、newで作成できる、通常インスペクター表示されない、Start/Update使えない、軽量で部品向き
{
    public string name;
    public int hp;
    public int maxHp;
    public int sp;
    public int maxSp = 100;
    public bool skillOnCooldown = false;
    public float cooldownTimer = 0f;
    public float cooldownDuration = 10f;
    private float spRecoveryTimer = 0f; // SP回復用タイマー

    // キャラを作る時に使うコンストラクタ
    public Character(string name, int hp, int maxHp)
    {
        this.name = name;
        this.hp = hp;
        this.maxHp = maxHp;
        this.sp = maxSp;
    }

    // キャラのHPを回復するメソッド
    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);// HPがmaxHpを超えないようにする
    }
    
    // キャラのHPを減らす(ダメージを受ける)メソッド
    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(hp - amount, 0);// HPが0未満にならないようにする
    }

    //HPの割合(0.0～1.0)を返す
    public float GetHpRatio()
    {
        if (maxHp <= 0) return 0f;
        return (float)hp / maxHp;
    }

    // SPの割合(0.0～1.0)を返す
    public float GetSpRatio()
    {
        if (maxSp <= 0) return 0f;
        return (float)sp / maxSp;
    }

    // クールダウン開始
    public void StartSkillCooldown()
    {
        skillOnCooldown = true;
        cooldownTimer = cooldownDuration;
        sp = Mathf.Clamp(0, 0, maxSp);
    }

    // クールダウン進行（毎フレーム呼び出し）
    public void UpdateCooldown(float deltaTime)
    {
        if (skillOnCooldown)
        {
            cooldownTimer -= deltaTime;
            spRecoveryTimer += deltaTime;

            // 1秒ごとにSPを0.1（maxSp/10）ずつ回復
            if (spRecoveryTimer >= 1f)
            {
                int spIncrease = Mathf.RoundToInt((float)maxSp / 10f);
                sp += spIncrease;
                sp = Mathf.Clamp(sp, 0, maxSp);
                spRecoveryTimer -= 1f;
            }

            if (cooldownTimer <= 0f)
            {
                skillOnCooldown = false;
                sp = maxSp;
                spRecoveryTimer = 0f;
            }
        }
        else
        {
            sp = maxSp; // クールダウンが終わったらSPは常に最大
            spRecoveryTimer = 0f;
        }
    }
}
