using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyUnit : MonoBehaviour
{
    [Header("ステータス")]
    public float hp = 8f;
    public float maxHP = 8f;
    public float moveSpeed = 80f;
    public int damage = 1;
    public float attackInterval = 1.2f;
    public float attackRange = 200f; // 交戦距離 (1キャラ分の隙間用)

    [Header("ビジュアル")]
    private Image unitImage;
    private Color originalColor;
    private bool isFlashing = false; // 追加

    [HideInInspector] public bool isMoving = true;
    private float attackTimer = 0f;

    void Start()
    {
        unitImage = GetComponent<Image>();
        if (unitImage != null) originalColor = unitImage.color;

        // サイズを100x100に固定（ユーザー要望）
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(100, 100);

        // BattleManagerに自分を登録
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.RegisterEnemy(this);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Battle) return;
        if (hp <= 0) Die();

        RectTransform rect = GetComponent<RectTransform>();
        bool reachedBase = rect != null && rect.anchoredPosition.x <= (BattleManager.Instance?.allyBaseX ?? -750f) + 50f;
        bool hasTarget = BattleManager.Instance != null && BattleManager.Instance.IsEnemyInRange(this);

        if (reachedBase || hasTarget)
        {
            isMoving = false;
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                TryAttack();
                attackTimer = 0f;
            }
        }
        else
        {
            isMoving = true;
            MoveBack();
            attackTimer = 0f;
        }
    }

    void MoveBack()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.x -= moveSpeed * Time.deltaTime;
            rectTransform.anchoredPosition = pos;
        }
    }

    void TryAttack()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null) return;

        // 1. 目の前の味方ユニットを攻撃
        if (BattleManager.Instance != null)
        {
            float myX = rect.anchoredPosition.x;
            SummonedUnit target = null;
            float minDistance = float.MaxValue;

            foreach (var ally in BattleManager.Instance.allyUnits)
            {
                if (ally == null) continue;
                float dist = Mathf.Abs(myX - ally.GetComponent<RectTransform>().anchoredPosition.x);
                if (dist < minDistance && dist <= BattleManager.Instance.attackRange)
                {
                    minDistance = dist;
                    target = ally;
                }
            }

            if (target != null)
            {
                Debug.Log($"{gameObject.name} attacks Ally Unit!");
                target.TakeDamage(damage); // TakeDamageを呼ぶように変更
                return;
            }
        }

        // 2. 味方ユニットがいなければ味方拠点を攻撃
        if (rect.anchoredPosition.x <= (BattleManager.Instance?.allyBaseX ?? -750f))
        {
            // 味方拠点（パーティ全員に均等ダメージ、またはランダムダメージ）
            if (GameManager.Instance != null && GameManager.Instance.partyMembers.Count > 0)
            {
                Debug.Log($"{gameObject.name} attacks Ally Base!");
                int targetIdx = Random.Range(0, GameManager.Instance.partyMembers.Count);
                GameManager.Instance.partyMembers[targetIdx].TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining HP: {hp}");
        if (unitImage != null) StartCoroutine(FlashRed());
        if (hp <= 0) Die();
    }

    IEnumerator FlashRed()
    {
        if (unitImage == null || isFlashing) yield break;
        isFlashing = true;
        Color oldColor = unitImage.color;
        unitImage.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (unitImage != null) unitImage.color = oldColor;
        isFlashing = false;
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
