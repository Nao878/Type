using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// キャラクターの見た目（Image）を制御し、ダメージ時の視覚効果を提供する
/// </summary>
public class CharacterVisual : MonoBehaviour
{
    public Image characterImage;//キャラのImage(色変化用)
    public Transform characterTransform;//キャラのオブジェクト(揺れ用)

    private Color originalColor;
    private Vector3 originalPosition;

    void Awake()//void Start()よりも早く読み込まれる
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
    /// ダメージを受けた際の視覚エフェクトを再生
    /// </summary>
    public void PlayDamageEffect()
    {
        StartCoroutine(FlashRed());
        StartCoroutine(Shake());
    }

    // Imageを一時的に赤くして戻す
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

    //小刻みに揺れる
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
