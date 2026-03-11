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

    private const string COIN_KEY = "HackCoins";
    private const string CHARA_KEY = "UnlockedCharacters";

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
        HackCoins = PlayerPrefs.GetInt(COIN_KEY, 0); // 初期コインは0
        
        string charaStr = PlayerPrefs.GetString(CHARA_KEY, "GlassMan"); // 初期キャラはGlassManのみ
        if (string.IsNullOrEmpty(charaStr))
        {
            UnlockedCharacters = new List<string> { "GlassMan" };
        }
        else
        {
            UnlockedCharacters = charaStr.Split(',').Distinct().ToList();
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt(COIN_KEY, HackCoins);
        PlayerPrefs.SetString(CHARA_KEY, string.Join(",", UnlockedCharacters));
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

    // デバッグ・リセット用
    public void ResetData()
    {
        PlayerPrefs.DeleteKey(COIN_KEY);
        PlayerPrefs.DeleteKey(CHARA_KEY);
        LoadData();
    }
}
