using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// �G�������_���ȊԊu�Ŗ����ɍU������X�N���v�g
public class EnemyAttack : MonoBehaviour
{
    public CharacterManager characterManager;
    public TypingManager typingManager;
    public List<CharacterVisual> hpUIControllers;

    public float minAttackInterval = 2f;
    public float maxAttackInterval = 5f;
    public int damage = 10;

    public HpUIController enemyHpUI; // �G���g��HP�o�[
    public int enemyHp = 100; // �ϓ�����G��HP
    public int maxEnemyHp = 100; // �G�̍ő�HP(�񕜎��ő�HP�𒴂��Ȃ��ׂ̌v�Z�ɗp����)

    public GameObject poisonEffectObj; // �ŏ�ԕ\���p�I�u�W�F�N�g
    public GameObject delayEffectObj; // �x����ԕ\���p�I�u�W�F�N�g
    public GameObject debuffEffectObj; // �f�o�t��ԕ\���p�I�u�W�F�N�g
    private bool isDebuffed = false;
    private float debuffTimer = 0f;
    private int debuffDamageMultiplier = 2;

    private bool isPoisoned = false;
    private float poisonTimer = 0f;
    private float poisonInterval = 1f;
    private float poisonTickTimer = 0f;
    private int poisonDamage = 1;

    private float delayedTime = 0f;

    //public bool IsGameOver {  get; private set; } = false;

    void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    // �Ō��ʂ�t�^���郁�\�b�h�i�p�����[�^�󂯎��j
    public void ApplyPoison(int damage, float interval, float duration)
    {
        isPoisoned = true;
        poisonTimer = duration;
        poisonInterval = interval;
        poisonTickTimer = 0f;
        poisonDamage = damage;
        if (poisonEffectObj != null) poisonEffectObj.SetActive(true);
    }

    public void AddDelay(float delay)
    {
        delayedTime += delay;
        if (delayEffectObj != null) delayEffectObj.SetActive(true);
    }

    public void ApplyDebuff(float duration, int damageMultiplier)
    {
        isDebuffed = true;
        debuffTimer = duration;
        debuffDamageMultiplier = damageMultiplier;
        if (debuffEffectObj != null) debuffEffectObj.SetActive(true);
    }

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

            Character target = characterManager.GetRandomAlly();
            if (target != null)
            {
                int actualDamage = isDebuffed ? damage * debuffDamageMultiplier : damage;
                target.TakeDamage(actualDamage);
                characterManager.UpdateAllHpUI();
                CharacterVisual visual = hpUIControllers[characterManager.partyMembers.IndexOf(target)].GetComponent<CharacterVisual>();
                if (visual != null)
                {
                    visual.PlayDamageEffect();
                }
            }

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
                if (poisonEffectObj != null) poisonEffectObj.SetActive(false);
            }
        }

        // �f�o�t��ԏ���
        if (isDebuffed)
        {
            debuffTimer -= Time.deltaTime;
            if (debuffTimer <= 0f)
            {
                isDebuffed = false;
                if (debuffEffectObj != null) debuffEffectObj.SetActive(false);
            }
        }
    }
}
