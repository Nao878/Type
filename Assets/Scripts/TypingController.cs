using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// リアルタイムタイピング入力を監視し、単語完成時にスキルを発動
/// </summary>
public class TypingController : MonoBehaviour
{
    [Header("UI参照")]
    public TMP_Text currentInputText;

    [Header("参照")]
    public SkillDatabase skillDatabase;
    public UIManager uiManager;

    private string currentInput = "";
    private bool inputEnabled = true;

    void Update()
    {
        if (!inputEnabled) return;
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameState.Battle) return;

        // キー入力を取得
        foreach (char c in Input.inputString)
        {
            ProcessCharacter(c);
        }
    }

    void ProcessCharacter(char c)
    {
        // バックスペース
        if (c == '\b')
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
            }
        }
        // Enter/Return
        else if (c == '\n' || c == '\r')
        {
            // 入力確定時にスキル発動をチェック
            TryActivateSkill();
        }
        // 通常文字（アルファベットのみ）
        else if (char.IsLetter(c))
        {
            currentInput += char.ToLower(c);

            // タイピングパーティクル演出
            if (uiManager != null)
            {
                uiManager.SpawnTypingParticle(c);
            }

            // リアルタイムでスキル発動チェック（Enterなしで発動）
            CheckWordCompletion();
        }

        // UI更新
        UpdateInputUI();
    }

    void CheckWordCompletion()
    {
        if (skillDatabase == null) return;

        // 現在の入力がスキル辞書に存在するか確認
        if (skillDatabase.HasSkill(currentInput))
        {
            skillDatabase.ActivateSkill(currentInput);
            Debug.Log($"Input: {currentInput}");
            ClearInput(); // Clear input and update UI
        }
    }

    void TryActivateSkill()
    {
        if (string.IsNullOrEmpty(currentInput)) return;

        if (skillDatabase != null && skillDatabase.HasSkill(currentInput))
        {
            skillDatabase.ActivateSkill(currentInput);
        }

        ClearInput(); // Clear input and update UI
    }

    void UpdateInputUI() // Renamed from UpdateInputDisplay
    {
        if (uiManager != null && uiManager.currentInputText != null)
        {
            uiManager.currentInputText.text = currentInput;
        }

        // サジェストの更新
        if (skillDatabase != null && uiManager != null)
        {
            var suggestions = skillDatabase.GetSuggestions(currentInput);
            uiManager.UpdateSuggestText(suggestions);
        }
    }

    public void DisableInput()
    {
        inputEnabled = false;
    }

    public void EnableInput()
    {
        inputEnabled = true;
    }

    public void ClearInput()
    {
        currentInput = "";
        UpdateInputUI();
    }

    /// <summary>
    /// スピードバフ中の入力緩和（曖昧検索）
    /// </summary>
    public bool IsSpeedBuffActive()
    {
        return GameManager.Instance != null && GameManager.Instance.isSpeedBuffActive;
    }
}
