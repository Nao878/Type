using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public CharacterManager characterManager;
    public TypingManager typingManager;

    public float minAttackInterval = 2f;
    public float maxAttackInterval = 5f;
    public int damage = 10;

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
            }

            if (characterManager.IsAllDead())
            {
                Debug.Log("Game Over!");
                yield break;
            }
        }
    }
}
