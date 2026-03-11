using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// プレイヤーのセーブデータ（所持コイン、解放済みキャラクター）を管理するクラス
/// PlayerPrefsを使用して簡単な永続化を行う
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public int HackCoins { get; private set; }
    public List<string> UnlockedCharacters { get; private set; } = new List<string>();
    public List<string> PartyFormation { get; private set; } = new List<string>();

    private const string COIN_KEY = "HackCoins";
    private const string CHARA_KEY = "UnlockedCharacters";
    private const string FORMATION_KEY = "PartyFormation";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadData()
    {
        HackCoins = PlayerPrefs.GetInt(COIN_KEY, 0);
        
        string charaStr = PlayerPrefs.GetString(CHARA_KEY, "GlassMan");
        if (string.IsNullOrEmpty(charaStr))
        {
            UnlockedCharacters = new List<string> { "GlassMan" };
        }
        else
        {
            UnlockedCharacters = charaStr.Split(',').Distinct().ToList();
        }

        // パーティ編成読み込み
        string formStr = PlayerPrefs.GetString(FORMATION_KEY, "");
        if (string.IsNullOrEmpty(formStr))
        {
            // 編成がない場合は解放済みキャラをそのまま編成に
            PartyFormation = new List<string>(UnlockedCharacters);
        }
        else
        {
            PartyFormation = formStr.Split(',').Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            // 解放済みに存在しないキャラを除外
            PartyFormation = PartyFormation.Where(c => UnlockedCharacters.Contains(c)).ToList();
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt(COIN_KEY, HackCoins);
        PlayerPrefs.SetString(CHARA_KEY, string.Join(",", UnlockedCharacters));
        PlayerPrefs.SetString(FORMATION_KEY, string.Join(",", PartyFormation));
        PlayerPrefs.Save();
    }

    public void AddCoins(int amount)
    {
        HackCoins += amount;
        SaveData();
    }

    public bool TryConsumeCoins(int amount)
    {
        if (HackCoins >= amount)
        {
            HackCoins -= amount;
            SaveData();
            return true;
        }
        return false;
    }

    public void UnlockCharacter(string charaName)
    {
        if (!UnlockedCharacters.Contains(charaName))
        {
            UnlockedCharacters.Add(charaName);
            SaveData();
        }
    }

    // パーティ編成を保存
    public void SavePartyFormation(List<string> formation)
    {
        PartyFormation = new List<string>(formation);
        SaveData();
    }

    // デバッグ・リセット用
    public void ResetData()
    {
        PlayerPrefs.DeleteKey(COIN_KEY);
        PlayerPrefs.DeleteKey(CHARA_KEY);
        PlayerPrefs.DeleteKey(FORMATION_KEY);
        LoadData();
    }
}
