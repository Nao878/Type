using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// 敵がランダムな間隔で味方に攻撃するスクリプト
public class EnemyAttack : MonoBehaviour
{
    public CharacterManager characterManager; // 味方キャラ管理
    public TypingManager typingManager; // タイピング管理
    public List<CharacterVisual> hpUIControllers; // 味方キャラのHPバー用Visual

    public float minAttackInterval = 2f; // 攻撃間隔の最小値
    public float maxAttackInterval = 5f; // 攻撃間隔の最大値
    public int damage = 10; // 通常攻撃ダメージ

    public HpUIController enemyHpUI; // 敵自身のHPバー
    public int enemyHp = 100; // 敵の現在HP
    public int maxEnemyHp = 100; // 敵の最大HP

    public GameObject poisonEffectObj; // 毒状態表示用オブジェクト
    public GameObject delayEffectObj; // 遅延状態表示用オブジェクト
    public GameObject debuffEffectObj; // デバフ状態表示用オブジェクト
    public GameObject effectParentObj; // エフェクトオブジェクトの親

    // デバフ状態管理
    private bool isDebuffed = false; // デバフ状態フラグ
    private float debuffTimer = 0f; // デバフ残り時間
    private int debuffDamageMultiplier = 2; // デバフ時のダメージ倍率

    // 毒状態管理
    private bool isPoisoned = false; // 毒状態フラグ
    private float poisonTimer = 0f; // 毒残り時間
    private float poisonInterval = 1f; // 毒ダメージ間隔
    private float poisonTickTimer = 0f; // 毒ダメージ用タイマー
    private int poisonDamage = 1; // 毒ダメージ量

    // 遅延状態管理
    private float delayedTime = 0f; // 累積遅延時間

    private List<GameObject> activeEffects = new List<GameObject>(); // 発動順で管理するエフェクトリスト

    // 毒効果を付与するメソッド（パラメータ受け取り）
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

    // 遅延効果を加算するメソッド
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

    // デバフ効果を付与するメソッド
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

    // エフェクトオブジェクトを横一列に並べる（発動順）
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

    // 発動順リストに追加
    private void AddEffectToActive(GameObject effectObj)
    {
        if (!activeEffects.Contains(effectObj))
            activeEffects.Add(effectObj);
    }
    // 発動順リストから除去
    private void RemoveEffectFromActive(GameObject effectObj)
    {
        activeEffects.Remove(effectObj);
    }

    void Start()
    {
        ArrangeEffectObjects(); // 初期配置
        // 攻撃ルーチン開始
        StartCoroutine(AttackRoutine());
    }

    // 敵の攻撃ルーチン
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

            // ランダムな味方キャラに攻撃
            Character target = characterManager.GetRandomAlly();
            if (target != null)
            {
                // デバフ状態ならダメージ2倍
                int actualDamage = isDebuffed ? damage * debuffDamageMultiplier : damage;
                target.TakeDamage(actualDamage);
                characterManager.UpdateAllHpUI();
                CharacterVisual visual = hpUIControllers[characterManager.partyMembers.IndexOf(target)].GetComponent<CharacterVisual>();
                if (visual != null)
                {
                    visual.PlayDamageEffect();
                }
            }

            // 全滅判定
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
                if (poisonEffectObj != null)
                {
                    poisonEffectObj.SetActive(false);
                    RemoveEffectFromActive(poisonEffectObj);
                    ArrangeEffectObjects();
                }
            }
        }

        // デバフ状態処理
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
