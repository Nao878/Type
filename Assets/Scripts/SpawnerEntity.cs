using UnityEngine;
using System.Collections;

/// <summary>
/// マクロ（自動実行）を担当するエントリ。
/// 記録されたスキルを一定間隔で実行し続ける。
/// </summary>
public class SpawnerEntity : SummonedUnit
{
    [Header("マクロ設定")]
    public string recordedSkill;
    public float interval = 4f;
    private float timer = 0f;

    public SkillDatabase skillDatabase;

    void Start()
    {
        unitType = UnitType.Stationary;
        base.Start();
        
        if (skillDatabase == null)
        {
            skillDatabase = FindObjectOfType<SkillDatabase>();
        }

        // 設置時に少し色を変えて区別（例：紫色）
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.color = new Color(0.7f, 0.3f, 1f);
        
        Debug.Log($"SpawnerEntity: Recorded [{recordedSkill}] spawned.");
    }

    void Update()
    {
        // 親クラス（SummonedUnit）のUpdate（HP管理等）も考慮しつつ、周期実行
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Battle) return;
        if (hp <= 0) Die();

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            ExecuteMacro();
            timer = 0f;
        }
    }

    void ExecuteMacro()
    {
        if (skillDatabase != null && !string.IsNullOrEmpty(recordedSkill))
        {
            Debug.Log($"SpawnerEntity: Auto-Executing [{recordedSkill}]");
            skillDatabase.ActivateSkill(recordedSkill);
            
            // 実行時に一瞬光らせる演出
            StartCoroutine(FlashEffect());
        }
    }

    IEnumerator FlashEffect()
    {
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img == null) yield break;
        Color original = img.color;
        img.color = Color.white;
        yield return new WaitForSeconds(0.2f);
        if (img != null) img.color = original;
    }
}
