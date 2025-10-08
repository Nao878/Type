using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ���[�U�[�̃^�C�s���O����������
public class TypingManager : MonoBehaviour
{
    public TMP_InputField inputField; // TextMeshPro�̓��̓t�B�[���h
    public EffectHandler effectHandler; // ���͂ɉ��������ʂ���������N���X(apple�Ɠ��́��񕜓�)
    public EnemyAttack enemyAttack; // �U����~�̂��߂ɎQ��

    void Start()
    {
        // ���̓t�B�[���h�̏�����(Enter���������Ƃ��A���͂���������)
        inputField.onEndEdit.AddListener(ProcessInput);
        inputField.ActivateInputField(); // �Q�[���J�n���ɓ��̓t�B�[���h���A�N�e�B�u�ɂ���
    }

    void ProcessInput(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            effectHandler.ProcessWord(input);// ���͂��ꂽ�P�����������
            inputField.text = ""; // ���̓t�B�[���h���N���A
            inputField.ActivateInputField(); // �ē��͉\�ɂ���
        }
    }

    public void DisableInput()
    {
        inputField.interactable = false; // ���̓t�B�[���h�𖳌���
        inputField.DeactivateInputField(); // ���̓t�B�[���h���A�N�e�B�u�ɂ���
    }
}
