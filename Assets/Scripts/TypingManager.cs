using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypingManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public EffectHandler effectHandler;

    void Start()
    {
        inputField.onEndEdit.AddListener(ProcessInput);
    }

    void ProcessInput(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            effectHandler.ProcessWord(input);
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
