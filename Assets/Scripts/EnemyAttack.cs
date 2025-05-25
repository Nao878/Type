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
                characterManager.UpdateAllHpUI(); // ��_���[�W���HP�X�V
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
