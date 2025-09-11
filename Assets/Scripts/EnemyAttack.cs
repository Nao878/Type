using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// 敵がランダムな間隔で味方に攻撃するスクリプト
public class EnemyAttack : MonoBehaviour
{
    public CharacterManager characterManager;
    public TypingManager typingManager;
    public List<CharacterVisual> hpUIControllers;

    public float minAttackInterval = 2f;
    public float maxAttackInterval = 5f;
    public int damage = 10;

    public HpUIController enemyHpUI; // 敵自身のHPバー
    public int enemyHp = 100; // 変動する敵のHP
    public int maxEnemyHp = 100; // 敵の最大HP(回復時最大HPを超えない為の計算に用いる)

    public GameObject poisonEffectObj; // 毒状態表示用オブジェクト
    public GameObject delayEffectObj; // 遅延状態表示用オブジェクト
    public GameObject debuffEffectObj; // デバフ状態表示用オブジェクト
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

    // 毒効果を付与するメソッド（パラメータ受け取り）
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
            // 遅延時間があれば待機
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
        // 毒状態処理
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

        // デバフ状態処理
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
