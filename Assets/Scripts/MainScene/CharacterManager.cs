using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine;

// �����L�����̊Ǘ��E����
public class CharacterManager : MonoBehaviour
{
    // ScriptableObject�ŃL�����N�^�[�f�[�^�Ǘ�
    public List<CharacterData> characterDataList; // �V�K�ǉ�
    public List<Character> partyMembers;
    public List<HpUIController> hpUIControllers; // HP�o�[UI���X�g�ƈ�v���Ă���K�v����
    public List<SpUIController> spUIControllers; // SP�o�[UI���X�g�ipartyMembers�ƈ�v�j
    public GameObject gameOverObj; // �S�Ŏ��ɃA�N�e�B�u�ɂ���I�u�W�F�N�g
    public List<CharacterVisual> characterVisuals; // �e�L�����̌����ڐ���

    void Awake()
    {
        // ScriptableObject����partyMembers��������
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

        // �S�Ŕ���
        if (IsAllDead() && gameOverObj != null)
        {
            gameOverObj.SetActive(true);
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

    // HP�o�[UI���X�V����
    public void UpdateAllHpUI()
    {
        int count = Mathf.Min(partyMembers.Count, hpUIControllers.Count);
        for (int i = 0; i < count; i++)
        {
            hpUIControllers[i].UpdateHpBar(partyMembers[i].GetHpRatio());
            // HP0�Ȃ�Â��A�����łȂ���Ό��̐F
            if (characterVisuals != null && characterVisuals.Count > i)
            {
                if (partyMembers[i].hp == 0)
                    characterVisuals[i].SetDeadVisual();
                else
                    characterVisuals[i].SetAliveVisual();
            }
        }
    }

    // SP�o�[UI���X�V����
    public void UpdateAllSpUI()
    {
        int count = Mathf.Min(partyMembers.Count, spUIControllers.Count);
        for (int i = 0; i < count; i++)
        {
            spUIControllers[i].UpdateSpBar(partyMembers[i].GetSpRatio());
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
    public int sp;
    public int maxSp = 100;
    public bool skillOnCooldown = false;
    public float cooldownTimer = 0f;
    public float cooldownDuration = 10f;
    private float spRecoveryTimer = 0f; // SP�񕜗p�^�C�}�[

    // �L��������鎞�Ɏg���R���X�g���N�^
    public Character(string name, int hp, int maxHp)
    {
        this.name = name;
        this.hp = hp;
        this.maxHp = maxHp;
        this.sp = maxSp;
    }

    // �L������HP���񕜂��郁�\�b�h
    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);// HP��maxHp�𒴂��Ȃ��悤�ɂ���
    }
    
    // �L������HP�����炷(�_���[�W���󂯂�)���\�b�h
    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(hp - amount, 0);// HP��0�����ɂȂ�Ȃ��悤�ɂ���
    }

    //HP�̊���(0.0�`1.0)��Ԃ�
    public float GetHpRatio()
    {
        if (maxHp <= 0) return 0f;
        return (float)hp / maxHp;
    }

    // SP�̊���(0.0�`1.0)��Ԃ�
    public float GetSpRatio()
    {
        if (maxSp <= 0) return 0f;
        return (float)sp / maxSp;
    }

    // �N�[���_�E���J�n
    public void StartSkillCooldown()
    {
        skillOnCooldown = true;
        cooldownTimer = cooldownDuration;
        sp = Mathf.Clamp(0, 0, maxSp);
    }

    // �N�[���_�E���i�s�i���t���[���Ăяo���j
    public void UpdateCooldown(float deltaTime)
    {
        if (skillOnCooldown)
        {
            cooldownTimer -= deltaTime;
            spRecoveryTimer += deltaTime;

            // 1�b���Ƃ�SP��0.1�imaxSp/10�j����
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
            sp = maxSp; // �N�[���_�E�����I�������SP�͏�ɍő�
            spRecoveryTimer = 0f;
        }
    }
}
