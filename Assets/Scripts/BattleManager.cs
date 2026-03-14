using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("全ユニットリスト")]
    public List<SummonedUnit> allyUnits = new List<SummonedUnit>();
    public List<EnemyUnit> enemyUnits = new List<EnemyUnit>();

    [Header("戦闘設定")]
    public float attackRange = 200f; // 100px幅のキャラ同士が100pxの隙間を空けて戦う距離 (50 + 100 + 50)
    public float allyBaseX = -750f;
    public float enemyBaseX = 750f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Battle) return;

        UpdateUnits();
    }

    void UpdateUnits()
    {
        // リストから破棄されたユニット（null）を除外
        allyUnits.RemoveAll(u => u == null);
        enemyUnits.RemoveAll(u => u == null);

        // 味方ユニットの更新
        foreach (var ally in allyUnits)
        {
            UpdateAllyUnitState(ally);
        }

        // 敵ユニットの更新
        foreach (var enemy in enemyUnits)
        {
            UpdateEnemyUnitState(enemy);
        }
    }

    void UpdateAllyUnitState(SummonedUnit ally)
    {
        RectTransform allyRect = ally.GetComponent<RectTransform>();
        if (allyRect == null) return;

        // 1. 敵ユニットとの距離をチェック
        EnemyUnit closestEnemy = FindClosestEnemy(allyRect.anchoredPosition.x);
        if (closestEnemy != null)
        {
            RectTransform enemyRect = closestEnemy.GetComponent<RectTransform>();
            float distance = Mathf.Abs(allyRect.anchoredPosition.x - enemyRect.anchoredPosition.x);

            if (distance <= attackRange)
            {
                ally.isMoving = false;
                // 攻撃ロジックの呼び出し（後でユニット側にメソッド追加予定）
                return;
            }
        }

        // 2. 敵拠点（ボス）との距離をチェック
        if (allyRect.anchoredPosition.x >= enemyBaseX)
        {
            ally.isMoving = false;
            // 拠点攻撃
            if (GameManager.Instance != null && GameManager.Instance.enemy != null)
            {
                // ここで直接ダメージを与えても良い
            }
            return;
        }

        // 3. 障害物がなければ前進
        ally.isMoving = true;
    }

    void UpdateEnemyUnitState(EnemyUnit enemy)
    {
        RectTransform enemyRect = enemy.GetComponent<RectTransform>();
        if (enemyRect == null) return;

        // 1. 味方ユニットとの距離をチェック
        SummonedUnit closestAlly = FindClosestAlly(enemyRect.anchoredPosition.x);
        if (closestAlly != null)
        {
            RectTransform allyRect = closestAlly.GetComponent<RectTransform>();
            float distance = Mathf.Abs(enemyRect.anchoredPosition.x - allyRect.anchoredPosition.x);

            if (distance <= attackRange)
            {
                enemy.isMoving = false;
                return;
            }
        }

        // 2. 味方拠点との距離をチェック
        if (enemyRect.anchoredPosition.x <= allyBaseX)
        {
            enemy.isMoving = false;
            // 味方拠点（プレイヤー）へダメージ
            return;
        }

        // 3. 障害物がなければ前進
        enemy.isMoving = true;
    }

    EnemyUnit FindClosestEnemy(float currentX)
    {
        EnemyUnit closest = null;
        float minDistance = float.MaxValue;

        foreach (var enemy in enemyUnits)
        {
            RectTransform rect = enemy.GetComponent<RectTransform>();
            float dist = Mathf.Abs(currentX - rect.anchoredPosition.x);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    SummonedUnit FindClosestAlly(float currentX)
    {
        SummonedUnit closest = null;
        float minDistance = float.MaxValue;

        foreach (var ally in allyUnits)
        {
            RectTransform rect = ally.GetComponent<RectTransform>();
            float dist = Mathf.Abs(currentX - rect.anchoredPosition.x);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = ally;
            }
        }
        return closest;
    }

    public bool IsAllyInRange(SummonedUnit ally)
    {
        RectTransform allyRect = ally.GetComponent<RectTransform>();
        if (allyRect == null) return false;
        EnemyUnit closest = FindClosestEnemy(allyRect.anchoredPosition.x);
        if (closest == null) return false;
        float distance = Mathf.Abs(allyRect.anchoredPosition.x - closest.GetComponent<RectTransform>().anchoredPosition.x);
        return distance <= attackRange;
    }

    public bool IsEnemyInRange(EnemyUnit enemy)
    {
        RectTransform enemyRect = enemy.GetComponent<RectTransform>();
        if (enemyRect == null) return false;
        SummonedUnit closest = FindClosestAlly(enemyRect.anchoredPosition.x);
        if (closest == null) return false;
        float distance = Mathf.Abs(enemyRect.anchoredPosition.x - closest.GetComponent<RectTransform>().anchoredPosition.x);
        return distance <= attackRange;
    }

    public void RegisterAlly(SummonedUnit unit) => allyUnits.Add(unit);
    public void RegisterEnemy(EnemyUnit unit) => enemyUnits.Add(unit);

    public void ClearAllUnits()
    {
        foreach (var ally in allyUnits)
        {
            if (ally != null) Destroy(ally.gameObject);
        }
        foreach (var enemy in enemyUnits)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }
        allyUnits.Clear();
        enemyUnits.Clear();
        Debug.Log("BattleManager: All units cleared.");
    }

}
