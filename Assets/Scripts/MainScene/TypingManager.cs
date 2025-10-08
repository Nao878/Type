using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ユーザーのタイピングを処理する
public class TypingManager : MonoBehaviour
{
    public TMP_InputField inputField; // TextMeshProの入力フィールド
    public EffectHandler effectHandler; // 入力に応じた効果を処理するクラス(appleと入力→回復等)
    public EnemyAttack enemyAttack; // 攻撃停止のために参照

    void Start()
    {
        // 入力フィールドの初期化(Enterを押したとき、入力を処理する)
        inputField.onEndEdit.AddListener(ProcessInput);
        inputField.ActivateInputField(); // ゲーム開始時に入力フィールドをアクティブにする
    }

    void ProcessInput(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            effectHandler.ProcessWord(input);// 入力された単語を処理する
            inputField.text = ""; // 入力フィールドをクリア
            inputField.ActivateInputField(); // 再入力可能にする
        }
    }

    public void DisableInput()
    {
        inputField.interactable = false; // 入力フィールドを無効化
        inputField.DeactivateInputField(); // 入力フィールドを非アクティブにする
    }
}
