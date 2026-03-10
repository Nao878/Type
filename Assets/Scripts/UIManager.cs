using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Canvas上のUI要素を管理
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("敵UI")]
    public Image enemyImage;
    public Image enemyHPBar;
    public TMP_Text enemyHPText;
    public GameObject poisonEffectIcon;
    public GameObject freezeEffectIcon;
    public GameObject slowEffectIcon;

    [Header("味方UI")]
    public List<Image> partyMemberImages;
    public List<Image> partyHPBars;
    public List<TMP_Text> partyHPTexts;
    public List<GameObject> protectEffectIcons;
    public List<Image> targetHighlights;

    [Header("タイピングUI")]
    public TMP_Text currentInputText;
    public TMP_Text skillActivationText;

    [Header("バフ表示")]
    public TMP_Text buffTimerText;
    public TMP_Text speedBuffTimerText;

    [Header("エフェクト")]
    public Transform typingParticleContainer;
    public TMP_Text criticalText;

    [Header("大技・発狂演出")]
    public GameObject dangerTextObj;
    public GameObject warningTextObj;
    public TMP_Text blockedText;

    [Header("ゲーム終了パネル")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("参照")]
    public GameManager gameManager;
    public Enemy enemy;

    private Coroutine skillActivationCoroutine;

    void Start()
    {
        // 初期状態でパネル非表示
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (poisonEffectIcon != null) poisonEffectIcon.SetActive(false);
        if (freezeEffectIcon != null) freezeEffectIcon.SetActive(false);
        if (slowEffectIcon != null) slowEffectIcon.SetActive(false);

        foreach (var icon in protectEffectIcons)
        {
            if (icon != null) icon.SetActive(false);
        }

        foreach (var highlight in targetHighlights)
        {
            if (highlight != null) highlight.gameObject.SetActive(false);
        }

        UpdateAllUI();
    }

    void Update()
    {
        // 状態異常アイコンの更新
        UpdateStatusEffectIcons();
        UpdateBuffTimerDisplay();

        // DANGERテキストの点滅処理
        if (dangerTextObj != null && dangerTextObj.activeSelf)
        {
            TMP_Text tmp = dangerTextObj.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                Color c = tmp.color;
                c.a = Mathf.PingPong(Time.time * 5f, 1f); // 激しく点滅
                tmp.color = c;
            }
        }
    }

    public void UpdateAllUI()
    {
        UpdateEnemyHP();
        UpdatePartyHP();
    }

    public void UpdateEnemyHP()
    {
        if (enemy == null) return;

        float ratio = enemy.GetHPRatio();
        if (enemyHPBar != null)
        {
            enemyHPBar.fillAmount = ratio;
        }

        if (enemyHPText != null)
        {
            enemyHPText.text = $"{enemy.currentHP}/{enemy.maxHP}";
        }
    }

    public void UpdatePartyHP()
    {
        if (gameManager == null) return;

        for (int i = 0; i < gameManager.partyMembers.Count && i < partyHPBars.Count; i++)
        {
            PartyMember member = gameManager.partyMembers[i];
            float ratio = member.GetHPRatio();

            if (partyHPBars[i] != null)
            {
                partyHPBars[i].fillAmount = ratio;
                // HPが低いと赤く
                partyHPBars[i].color = ratio > 0.3f ? Color.green : Color.red;
            }

            if (i < partyHPTexts.Count && partyHPTexts[i] != null)
            {
                partyHPTexts[i].text = $"{member.currentHP}/{member.maxHP}";
            }

            // 死亡時はグレーアウト
            if (i < partyMemberImages.Count && partyMemberImages[i] != null)
            {
                partyMemberImages[i].color = member.currentHP > 0 ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            // 無敵表示
            if (i < protectEffectIcons.Count && protectEffectIcons[i] != null)
            {
                protectEffectIcons[i].SetActive(member.isInvincible);
            }
        }
    }

    void UpdateStatusEffectIcons()
    {
        if (enemy == null) return;

        if (poisonEffectIcon != null)
            poisonEffectIcon.SetActive(enemy.isPoisoned);

        if (freezeEffectIcon != null)
            freezeEffectIcon.SetActive(enemy.isFrozen);

        if (slowEffectIcon != null)
            slowEffectIcon.SetActive(enemy.isSlowed);
    }

    void UpdateBuffTimerDisplay()
    {
        if (gameManager == null) return;

        if (buffTimerText != null)
        {
            if (gameManager.isBuffActive)
            {
                buffTimerText.text = $"BUFF: {gameManager.buffTimer:F1}s";
                buffTimerText.gameObject.SetActive(true);
            }
            else
            {
                buffTimerText.gameObject.SetActive(false);
            }
        }

        if (speedBuffTimerText != null)
        {
            if (gameManager.isSpeedBuffActive)
            {
                speedBuffTimerText.text = $"SPEED: {gameManager.speedBuffTimer:F1}s";
                speedBuffTimerText.gameObject.SetActive(true);
            }
            else
            {
                speedBuffTimerText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateBuffDisplay(string buffName, float remainingTime)
    {
        // バフ表示更新（UpdateBuffTimerDisplayで処理）
    }

    public void ShowSkillActivation(string skillName)
    {
        if (skillActivationText != null)
        {
            if (skillActivationCoroutine != null)
            {
                StopCoroutine(skillActivationCoroutine);
            }
            skillActivationCoroutine = StartCoroutine(ShowSkillActivationCoroutine(skillName));
        }
    }

    IEnumerator ShowSkillActivationCoroutine(string skillName)
    {
        skillActivationText.text = $"[{skillName.ToUpper()}]";
        skillActivationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        skillActivationText.gameObject.SetActive(false);
    }

    public void ShowDamageEffect(int partyIndex)
    {
        if (partyIndex >= 0 && partyIndex < partyMemberImages.Count)
        {
            StartCoroutine(FlashRed(partyMemberImages[partyIndex]));
        }
    }

    IEnumerator FlashRed(Image image)
    {
        if (image == null) yield break;

        Color original = image.color;
        image.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        image.color = original;
    }

    public void ShowProtectEffect(int partyIndex)
    {
        if (partyIndex >= 0 && partyIndex < protectEffectIcons.Count && protectEffectIcons[partyIndex] != null)
        {
            protectEffectIcons[partyIndex].SetActive(true);
        }
    }

    public void ShowPoisonEffect()
    {
        if (poisonEffectIcon != null)
        {
            poisonEffectIcon.SetActive(true);
        }
    }

    public void ShowFreezeEffect()
    {
        if (freezeEffectIcon != null)
        {
            freezeEffectIcon.SetActive(true);
        }
    }

    public void HighlightNextTarget(int partyIndex)
    {
        // 全てのハイライトを消す
        foreach (var highlight in targetHighlights)
        {
            if (highlight != null) highlight.gameObject.SetActive(false);
        }

        // 指定されたキャラをハイライト
        if (partyIndex >= 0 && partyIndex < targetHighlights.Count && targetHighlights[partyIndex] != null)
        {
            targetHighlights[partyIndex].gameObject.SetActive(true);
            StartCoroutine(HideHighlightAfterDelay(partyIndex, 3f));
        }
    }

    IEnumerator HideHighlightAfterDelay(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (index < targetHighlights.Count && targetHighlights[index] != null)
        {
            targetHighlights[index].gameObject.SetActive(false);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void ShowVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    // ========================================
    // タイピングパーティクル演出
    // ========================================

    /// <summary>
    /// 入力された文字を上方にフロート＆フェードアウトさせるパーティクル演出
    /// </summary>
    public void SpawnTypingParticle(char c)
    {
        if (typingParticleContainer == null) return;

        // テキストオブジェクトを動的に生成
        GameObject particleObj = new GameObject("TypingParticle");
        particleObj.transform.SetParent(typingParticleContainer);

        RectTransform rect = particleObj.AddComponent<RectTransform>();
        // 入力欄付近のランダムなX位置から出現
        float randomX = Random.Range(-100f, 100f);
        rect.anchoredPosition = new Vector2(randomX, 0f);
        rect.sizeDelta = new Vector2(50, 50);
        // ランダムな回転で遊び心を追加
        rect.localRotation = Quaternion.Euler(0, 0, Random.Range(-25f, 25f));
        rect.localScale = Vector3.one * Random.Range(0.8f, 1.4f);

        TextMeshProUGUI tmp = particleObj.AddComponent<TextMeshProUGUI>();
        tmp.text = c.ToString().ToUpper();
        tmp.fontSize = Random.Range(28, 48);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(
            Random.Range(0.6f, 1f),
            Random.Range(0.8f, 1f),
            Random.Range(0.3f, 1f),
            1f
        );
        tmp.raycastTarget = false;

        StartCoroutine(AnimateTypingParticle(particleObj, rect, tmp));
    }

    IEnumerator AnimateTypingParticle(GameObject obj, RectTransform rect, TMP_Text tmp)
    {
        float duration = 0.8f;
        float elapsed = 0f;
        Vector2 startPos = rect.anchoredPosition;
        // やや左右にブレつつ上に飛ぶ
        float driftX = Random.Range(-30f, 30f);
        Color startColor = tmp.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // イーズアウトで上方に移動
            float easedT = 1f - (1f - t) * (1f - t);
            rect.anchoredPosition = startPos + new Vector2(driftX * t, 120f * easedT);

            // フェードアウト（後半で加速）
            float alpha = Mathf.Lerp(1f, 0f, t * t);
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            // 微妙にスケールダウン
            rect.localScale = Vector3.one * Mathf.Lerp(rect.localScale.x, 0.3f, t);

            yield return null;
        }

        Destroy(obj);
    }

    // ========================================
    // クリティカル演出（believe スキル用）
    // ========================================

    /// <summary>
    /// CRITICAL!! を画面中央にど派手に表示し、キャンバスをシェイクさせる
    /// </summary>
    public void ShowCriticalEffect()
    {
        StartCoroutine(CriticalEffectCoroutine());
    }

    IEnumerator CriticalEffectCoroutine()
    {
        // === カメラ/キャンバス シェイク（0.2秒） ===
        RectTransform canvasRect = typingParticleContainer?.root?.GetComponent<RectTransform>();
        StartCoroutine(ShakeCanvas(canvasRect, 0.2f, 15f));

        // === CRITICAL!! テキスト表示（0.5秒） ===
        if (criticalText != null)
        {
            criticalText.text = "CRITICAL!!";
            criticalText.gameObject.SetActive(true);

            // スケールバウンスアニメーション（ド派手に）
            RectTransform critRect = criticalText.GetComponent<RectTransform>();
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 最初の0.15秒で巨大にズームイン、その後少し縮む
                float scale;
                if (t < 0.3f)
                {
                    // 0→1.5 に急拡大
                    scale = Mathf.Lerp(0f, 1.5f, t / 0.3f);
                }
                else
                {
                    // 1.5→1.0 に戻る
                    scale = Mathf.Lerp(1.5f, 1.0f, (t - 0.3f) / 0.7f);
                }
                critRect.localScale = Vector3.one * scale;

                // 後半でフェードアウト
                float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                criticalText.color = new Color(1f, 0.1f, 0.1f, alpha);

                yield return null;
            }

            criticalText.gameObject.SetActive(false);
            critRect.localScale = Vector3.one;
        }
    }

    IEnumerator ShakeCanvas(RectTransform canvasRect, float duration, float magnitude)
    {
        if (canvasRect == null) yield break;

        Vector2 originalPos = canvasRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 徐々に減衰するシェイク
            float currentMag = magnitude * (1f - t);
            float offsetX = Random.Range(-currentMag, currentMag);
            float offsetY = Random.Range(-currentMag, currentMag);
            canvasRect.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
            yield return null;
        }

        canvasRect.anchoredPosition = originalPos;
    }

    // ========================================
    // 大技・発狂演出
    // ========================================

    public void ShowDangerText(bool show)
    {
        if (dangerTextObj != null)
        {
            dangerTextObj.SetActive(show);
        }
    }

    public void ShowWarningText(bool show)
    {
        if (warningTextObj != null)
        {
            warningTextObj.SetActive(show);
        }
    }

    public void ShowBlockedEffect()
    {
        if (blockedText != null)
        {
            StartCoroutine(BlockedEffectCoroutine());
        }
    }

    IEnumerator BlockedEffectCoroutine()
    {
        blockedText.text = "BLOCKED!";
        blockedText.gameObject.SetActive(true);

        RectTransform rect = blockedText.GetComponent<RectTransform>();
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // バウンドスケール
            float scale = Mathf.Lerp(0.5f, 1.2f, Mathf.Sin(t * Mathf.PI));
            rect.localScale = Vector3.one * scale;

            // 後半でフェードアウト
            float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            blockedText.color = new Color(0f, 1f, 1f, alpha); // シアン色

            yield return null;
        }

        blockedText.gameObject.SetActive(false);
        rect.localScale = Vector3.one;
    }
}
