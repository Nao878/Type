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
            inputField.text = ""; // ���̓t�B�[���h���N���A
            inputField.ActivateInputField(); // �ē��͉\�ɂ���
        }
    }
}
