using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �L�����N�^�[�̌����ځiImage�j�𐧌䂵�A�_���[�W���̎��o���ʂ�񋟂���
/// </summary>
public class CharacterVisual : MonoBehaviour
{
    public Image characterImage;//�L������Image(�F�ω��p)
    public Transform characterTransform;//�L�����̃I�u�W�F�N�g(�h��p)

    private Color originalColor;
    private Vector3 originalPosition;

    void Awake()//void Start()���������ǂݍ��܂��
    {
        if (characterImage != null)
        {
            originalColor = characterImage.color;
        }

        if (characterTransform != null)
        {
            originalPosition = characterTransform.localPosition;
        }
    }

    /// <summary>
    /// �_���[�W���󂯂��ۂ̎��o�G�t�F�N�g���Đ�
    /// </summary>
    public void PlayDamageEffect()
    {
        StartCoroutine(FlashRed());
        StartCoroutine(Shake());
    }

    // Image���ꎞ�I�ɐԂ����Ė߂�
    IEnumerator FlashRed()
    {
        if (characterImage == null)
        {
            yield break;
        }
        characterImage.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        characterImage.color = originalColor;
    }

    //�����݂ɗh���
    IEnumerator Shake()
    {
        if (characterTransform == null)
        {
            yield break;
        }

        float duration = 0.3f;
        float magnitude = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            characterTransform.localPosition = originalPosition + new Vector3(x, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        characterTransform.localPosition = originalPosition;
    }
}
