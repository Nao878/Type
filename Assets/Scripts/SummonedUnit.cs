using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum UnitType
{
    Mobile,
    Stationary,
    Instant
}

public class SummonedUnit : MonoBehaviour
{
    [Header("タイプ")]
    public UnitType unitType = UnitType.Mobile;

    [Header("ステータス")]
    public float hp = 10f;
    public float maxHP = 10f; // 追加
    public float moveSpeed = 100f;
    public int damage = 2;
    public float attackInterval = 1.0f;
    public float attackRange = 200f; // 交戦距離 (1キャラ分の隙間用)

    [Header("ビジュアル")]
    private Image unitImage;
    private Color originalColor;
    private bool isFlashing = false; // 追加

    [HideInInspector] public bool isMoving = true;
    private float attackTimer = 0f;

    protected virtual void Start()
    {
        unitImage = GetComponent<Image>();
        if (unitImage != null) originalColor = unitImage.color;

        // BattleManagerに自分を登録
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.RegisterAlly(this);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Battle) return;
        if (hp <= 0) Die();

        // 攻撃対象がいるか、ボスに到達したか
        bool hasTarget = BattleManager.Instance != null && BattleManager.Instance.IsAllyInRange(this);
        RectTransform rect = GetComponent<RectTransform>();
        bool reachedBoss = rect != null && rect.anchoredPosition.x >= (BattleManager.Instance?.enemyBaseX ?? 750f) - 50f;

        if (hasTarget || reachedBoss || unitType == UnitType.Stationary)
        {
            isMoving = false;
            // 移動停止中は移動・攻撃タイマーを進める
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                PerformAttack(reachedBoss);
                attackTimer = 0f;
            }
        }
        else
        {
            isMoving = true;
            MoveForward();
            attackTimer = 0f;
        }
    }

    void MoveForward()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.x += moveSpeed * Time.deltaTime;
            rectTransform.anchoredPosition = pos;
        }
    }

    void PerformAttack(bool isTargetBoss)
    {
        if (isTargetBoss)
        {
            if (GameManager.Instance != null && GameManager.Instance.enemy != null)
            {
                Debug.Log($"{gameObject.name} attacks Enemy Boss!");
                GameManager.Instance.enemy.TakeDamage(damage);
            }
        }
        else
        {
            // ユニットへの攻撃（最寄りの敵を攻撃）
            TryAttackUnit();
        }
    }

    void TryAttackUnit()
    {
        if (BattleManager.Instance == null) return;
        
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null) return;
        float myX = rect.anchoredPosition.x;
        EnemyUnit target = null;
        float minDistance = float.MaxValue;

        foreach (var enemy in BattleManager.Instance.enemyUnits)
        {
            if (enemy == null) continue;
            float dist = Mathf.Abs(myX - enemy.GetComponent<RectTransform>().anchoredPosition.x);
            if (dist < minDistance && dist <= BattleManager.Instance.attackRange)
            {
                minDistance = dist;
                target = enemy;
            }
        }

        if (target != null)
        {
            Debug.Log($"{gameObject.name} attacks Enemy Unit!");
            target.TakeDamage(damage); // hpを直接減らさずTakeDamageを呼ぶ
        }
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
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
        // 破棄される際にリストから消えるのはBattleManager側でハンドリング
        Destroy(gameObject);
    }
}
