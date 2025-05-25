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

    //public bool IsGameOver {  get; private set; } = false;

    void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minAttackInterval, maxAttackInterval);
            yield return new WaitForSeconds(waitTime);

            Character target = characterManager.GetRandomAlly();
            if (target != null)
            {
                target.TakeDamage(damage);
                characterManager.UpdateAllHpUI(); // 被ダメージ後にHP更新
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
}
