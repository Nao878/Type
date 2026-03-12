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

    public bool HasSeenTutorialStory1 { get; private set; }
    public bool HasSeenTutorialStory2 { get; private set; }

    private const string COIN_KEY = "HackCoins";
    private const string CHARA_KEY = "UnlockedCharacters";
    private const string FORMATION_KEY = "PartyFormation";
    private const string STORY1_KEY = "HasSeenTutorialStory1";
    private const string STORY2_KEY = "HasSeenTutorialStory2";

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

        HasSeenTutorialStory1 = PlayerPrefs.GetInt(STORY1_KEY, 0) == 1;
        HasSeenTutorialStory2 = PlayerPrefs.GetInt(STORY2_KEY, 0) == 1;
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt(COIN_KEY, HackCoins);
        PlayerPrefs.SetString(CHARA_KEY, string.Join(",", UnlockedCharacters));
        PlayerPrefs.SetString(FORMATION_KEY, string.Join(",", PartyFormation));
        PlayerPrefs.SetInt(STORY1_KEY, HasSeenTutorialStory1 ? 1 : 0);
        PlayerPrefs.SetInt(STORY2_KEY, HasSeenTutorialStory2 ? 1 : 0);
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

    public void MarkTutorialStory1Seen()
    {
        HasSeenTutorialStory1 = true;
        SaveData();
    }

    public void MarkTutorialStory2Seen()
    {
        HasSeenTutorialStory2 = true;
        SaveData();
    }

    // デバッグ・リセット用
    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        LoadData();
    }
}
