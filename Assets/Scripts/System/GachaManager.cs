using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ガチャシステムを管理するクラス
/// </summary>
public class GachaManager : MonoBehaviour
{
    public UIManager uiManager;
    public GameManager gameManager;
    
    public int GachaCost = 100; // 1回引くための必要コイン
    
    // ゲーム内に存在する全キャラクター名のリスト
    private readonly List<string> allCharacters = new List<string>
    {
        "GlassMan",
        "Gentleman",
        "CatGirl",
        "YellowGirl"
    };

    /// <summary>
    /// ガチャを1回引く処理
    /// </summary>
    public void DrawGacha()
    {
        if (PlayerDataManager.Instance == null) return;
        
        // 未解放キャラのリストアップ
        List<string> lockedChars = allCharacters
            .Where(c => !PlayerDataManager.Instance.UnlockedCharacters.Contains(c))
            .ToList();

        if (lockedChars.Count == 0)
        {
            // 全キャラコンプリート済み
            if (uiManager != null)
            {
                uiManager.ShowGachaResult("You have unlocked everyone!");
            }
            return;
        }

        // コイン消費
        if (PlayerDataManager.Instance.TryConsumeCoins(GachaCost))
        {
            // ランダム抽選
            int randomIndex = Random.Range(0, lockedChars.Count);
            string newChara = lockedChars[randomIndex];
            
            // 解放処理
            PlayerDataManager.Instance.UnlockCharacter(newChara);
            
            // UI演出
            if (uiManager != null)
            {
                uiManager.ShowGachaResult($"{newChara} RECOVERED!", newChara);
                uiManager.UpdateCoinText(); // コイン表示の更新
            }
        }
        else
        {
            // コイン不足
            if (uiManager != null)
            {
                uiManager.ShowGachaResult("Not enough Hack Coins!");
            }
        }
    }

    /// <summary>
    /// ガチャ画面を閉じてリザルト（またはタイトル）へ戻る
    /// </summary>
    public void CloseGacha()
    {
        if (uiManager != null)
        {
            uiManager.HideGachaPanel();
        }
        
        // ホーム画面へ戻る
        if (gameManager != null)
        {
            gameManager.GoToHome();
        }
    }
}
