using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("グリッド設定")]
    public int gridCount = 10;
    public float gridWidth = 120f;
    public float startX = -600f; // 拠点付近から右へ
    public float gridY = -200f;

    private Dictionary<int, SummonedUnit> gridOccupants = new Dictionary<int, SummonedUnit>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 最前線（最も右のMobileユニット）に基づいた配置スロットを返す
    /// </summary>
    public int GetFrontlineSlot()
    {
        if (BattleManager.Instance == null) return 0;

        float maxMobileX = startX;
        bool foundMobile = false;

        foreach (var ally in BattleManager.Instance.allyUnits)
        {
            if (ally != null && ally.unitType == UnitType.Mobile)
            {
                float x = ally.GetComponent<RectTransform>().anchoredPosition.x;
                if (x > maxMobileX)
                {
                    maxMobileX = x;
                    foundMobile = true;
                }
            }
        }

        if (!foundMobile) return 0;

        // X座標からスロットインデックスへ変換
        int slot = Mathf.FloorToInt((maxMobileX - startX) / gridWidth);
        // 「すぐ後ろ」なので -1
        return Mathf.Clamp(slot - 1, 0, gridCount - 1);
    }

    public Vector2 GetSlotPosition(int slot)
    {
        return new Vector2(startX + (slot * gridWidth), gridY);
    }

    public void PlaceUnit(SummonedUnit unit, int slot)
    {
        if (gridOccupants.ContainsKey(slot) && gridOccupants[slot] != null)
        {
            // 進化コンボチェック
            HandleCombo(gridOccupants[slot], unit, slot);
            return;
        }

        gridOccupants[slot] = unit;
        RectTransform rt = unit.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = GetSlotPosition(slot);
    }

    private void HandleCombo(SummonedUnit existing, SummonedUnit newUnit, int slot)
    {
        string comboKey = $"{existing.gameObject.name}_{newUnit.gameObject.name}".ToLower();
        Debug.Log($"Combo Check: {comboKey}");

        // 例: wall + wall -> ironwall
        if (existing.gameObject.name.Contains("wall") && newUnit.gameObject.name.Contains("wall"))
        {
            existing.hp *= 2; // HP倍増
            existing.maxHP *= 2;
            existing.gameObject.name = "Unit_ironwall";
            existing.GetComponent<UnityEngine.UI.Image>().color = Color.black;
            Destroy(newUnit.gameObject);
            Debug.Log("COMBO: ironwall created!");
        }
        // 例: wall + fire -> firewall
        else if (existing.gameObject.name.Contains("wall") && newUnit.gameObject.name.Contains("fire"))
        {
            existing.damage += 5; // 接触ダメージ追加
            existing.gameObject.name = "Unit_firewall";
            existing.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 0.5f, 0); // オレンジ
            Destroy(newUnit.gameObject);
            Debug.Log("COMBO: firewall created!");
        }
        else
        {
            // 重ねられない場合は既存を壊して新しいのを置く（または何もしない）
            Destroy(existing.gameObject);
            gridOccupants[slot] = newUnit;
            newUnit.GetComponent<RectTransform>().anchoredPosition = GetSlotPosition(slot);
        }
    }

    void Update()
    {
        // 死亡したユニットを辞書から削除
        var keys = gridOccupants.Keys.ToList();
        foreach (var k in keys)
        {
            if (gridOccupants[k] == null) gridOccupants.Remove(k);
        }
    }
}
