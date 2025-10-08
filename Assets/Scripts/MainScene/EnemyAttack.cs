using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// �G�������_���ȊԊu�Ŗ����ɍU������X�N���v�g
public class EnemyAttack : MonoBehaviour
{
    public CharacterManager characterManager; // �����L�����Ǘ�
    public TypingManager typingManager; // �^�C�s���O�Ǘ�
    public List<CharacterVisual> hpUIControllers; // �����L������HP�o�[�pVisual

    public float minAttackInterval = 2f; // �U���Ԋu�̍ŏ��l
    public float maxAttackInterval = 5f; // �U���Ԋu�̍ő�l
    public int damage = 10; // �ʏ�U���_���[�W

    public HpUIController enemyHpUI; // �G���g��HP�o�[
    public int enemyHp = 100; // �G�̌���HP
    public int maxEnemyHp = 100; // �G�̍ő�HP

    public GameObject poisonEffectObj; // �ŏ�ԕ\���p�I�u�W�F�N�g
    public GameObject delayEffectObj; // �x����ԕ\���p�I�u�W�F�N�g
    public GameObject debuffEffectObj; // �f�o�t��ԕ\���p�I�u�W�F�N�g
    public GameObject effectParentObj; // �G�t�F�N�g�I�u�W�F�N�g�̐e

    // �f�o�t��ԊǗ�
    private bool isDebuffed = false; // �f�o�t��ԃt���O
    private float debuffTimer = 0f; // �f�o�t�c�莞��
    private int debuffDamageMultiplier = 2; // �f�o�t���̃_���[�W�{��

    // �ŏ�ԊǗ�
    private bool isPoisoned = false; // �ŏ�ԃt���O
    private float poisonTimer = 0f; // �Ŏc�莞��
    private float poisonInterval = 1f; // �Ń_���[�W�Ԋu
    private float poisonTickTimer = 0f; // �Ń_���[�W�p�^�C�}�[
    private int poisonDamage = 1; // �Ń_���[�W��

    // �x����ԊǗ�
    private float delayedTime = 0f; // �ݐϒx������

    private List<GameObject> activeEffects = new List<GameObject>(); // �������ŊǗ�����G�t�F�N�g���X�g

    // �Ō��ʂ�t�^���郁�\�b�h�i�p�����[�^�󂯎��j
    public void ApplyPoison(int damage, float interval, float duration)
    {
        isPoisoned = true;
        poisonTimer = duration;
        poisonInterval = interval;
        poisonTickTimer = 0f;
        poisonDamage = damage;
        if (poisonEffectObj != null)
        {
            poisonEffectObj.SetActive(true);
            AddEffectToActive(poisonEffectObj);
        }
        ArrangeEffectObjects();
    }

    // �x�����ʂ����Z���郁�\�b�h
    public void AddDelay(float delay)
    {
        delayedTime += delay;
        if (delayEffectObj != null)
        {
            delayEffectObj.SetActive(true);
            AddEffectToActive(delayEffectObj);
        }
        ArrangeEffectObjects();
    }

    // �f�o�t���ʂ�t�^���郁�\�b�h
    public void ApplyDebuff(float duration, int damageMultiplier)
    {
        isDebuffed = true;
        debuffTimer = duration;
        debuffDamageMultiplier = damageMultiplier;
        if (debuffEffectObj != null)
        {
            debuffEffectObj.SetActive(true);
            AddEffectToActive(debuffEffectObj);
        }
        ArrangeEffectObjects();
    }

    // �G�t�F�N�g�I�u�W�F�N�g�������ɕ��ׂ�i�������j
    private void ArrangeEffectObjects()
    {
        float spacing = 10f;
        Vector3 basePos = effectParentObj != null ? effectParentObj.transform.position : Vector3.zero;
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] != null)
            {
                activeEffects[i].transform.position = basePos + new Vector3(i * spacing, 0, 0);
                activeEffects[i].transform.SetParent(effectParentObj.transform, false);
            }
        }
    }

    // ���������X�g�ɒǉ�
    private void AddEffectToActive(GameObject effectObj)
    {
        if (!activeEffects.Contains(effectObj))
            activeEffects.Add(effectObj);
    }
    // ���������X�g���珜��
    private void RemoveEffectFromActive(GameObject effectObj)
    {
        activeEffects.Remove(effectObj);
    }

    void Start()
    {
        ArrangeEffectObjects(); // �����z�u
        // �U�����[�`���J�n
        StartCoroutine(AttackRoutine());
    }

    // �G�̍U�����[�`��
    IEnumerator AttackRoutine()
    {
        while (true)
        {
            // �x�����Ԃ�����Αҋ@
            if (delayedTime > 0f)
            {
                if (delayEffectObj != null) delayEffectObj.SetActive(true);
                yield return new WaitForSeconds(delayedTime);
                delayedTime = 0f;
                if (delayEffectObj != null) delayEffectObj.SetActive(false);
            }

            float waitTime = Random.Range(minAttackInterval, maxAttackInterval);
            yield return new WaitForSeconds(waitTime);

            // �����_���Ȗ����L�����ɍU��
            Character target = characterManager.GetRandomAlly();
            if (target != null)
            {
                // �f�o�t��ԂȂ�_���[�W2�{
                int actualDamage = isDebuffed ? damage * debuffDamageMultiplier : damage;
                target.TakeDamage(actualDamage);
                characterManager.UpdateAllHpUI();
                CharacterVisual visual = hpUIControllers[characterManager.partyMembers.IndexOf(target)].GetComponent<CharacterVisual>();
                if (visual != null)
                {
                    visual.PlayDamageEffect();
                }
            }

            // �S�Ŕ���
            if (characterManager.IsAllDead())
            {
                Debug.Log("Game Over!");
                yield break;
            }
        }
    }

    void Update()
    {
        // �ŏ�ԏ���
        if (isPoisoned)
        {
            poisonTimer -= Time.deltaTime;
            poisonTickTimer += Time.deltaTime;
            if (poisonTickTimer >= poisonInterval)
            {
                enemyHp = Mathf.Max(enemyHp - poisonDamage, 0);
                poisonTickTimer -= poisonInterval;
                if (enemyHpUI != null)
                    enemyHpUI.UpdateHpBar((float)enemyHp / maxEnemyHp);
            }
            if (poisonTimer <= 0f)
            {
                isPoisoned = false;
                if (poisonEffectObj != null)
                {
                    poisonEffectObj.SetActive(false);
                    RemoveEffectFromActive(poisonEffectObj);
                    ArrangeEffectObjects();
                }
            }
        }

        // �f�o�t��ԏ���
        if (isDebuffed)
        {
            debuffTimer -= Time.deltaTime;
            if (debuffTimer <= 0f)
            {
                isDebuffed = false;
                if (debuffEffectObj != null)
                {
                    debuffEffectObj.SetActive(false);
                    RemoveEffectFromActive(debuffEffectObj);
                    ArrangeEffectObjects();
                }
            }
        }
    }
}
