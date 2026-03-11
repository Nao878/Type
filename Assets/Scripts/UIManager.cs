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
    public List<TMP_Text> partySkillTexts;
    public List<GameObject> partyMemberAreas;

    [Header("タイピングUI")]
    public TMP_Text currentInputText;
    public TMP_Text skillActivationText;
    public TMP_Text suggestText;

    [Header("バフ表示")]
    public TMP_Text buffTimerText;
    public TMP_Text speedBuffTimerText;

    [Header("エフェクト")]
    public Transform typingParticleContainer;
    public TMP_Text criticalText;
    public TMP_FontAsset mainFont;

    [Header("大技・発狂演出")]
    public GameObject dangerTextObj;
    public GameObject warningTextObj;
    public TMP_Text blockedText;

    [Header("コンボ演出")]
    public TMP_Text comboText;

    [Header("ゲーム終了パネル")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("ポーズ・スキル辞書UI")]
    public GameObject skillDictionaryPanel;
    public TMP_Text skillDictionaryText;
    public UnityEngine.UI.Button pauseButton;
    public UnityEngine.UI.Button resumeButton;

    [Header("チュートリアル")]
    public TMP_Text tutorialText;
    private bool hasShownAttackTutorial = false;
    private bool hasShownHealTutorial = false;
    private Coroutine hideTutorialCoroutine;

    [Header("ホーム画面UI")]
    public GameObject homePanel;
    public TMP_Text homeCoinText;

    [Header("ガチャ関連UI")]
    public GameObject gachaPanel;
    public TMP_Text coinText; // 現在のコイン表示
    public GameObject gachaResultPanel; // 結果表示用サブパネル
    public TMP_Text gachaResultText;
    public Image gachaResultImage;
    public TMP_Text victoryCoinText; // リザルト画面で獲得コインを表示する用

    [Header("参照")]
    public GameManager gameManager;
    public Enemy enemy;

    private Coroutine skillActivationCoroutine;

    void Start()
    {
        // 初期状態でパネル非表示
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (skillDictionaryPanel != null) skillDictionaryPanel.SetActive(false);
        if (gachaPanel != null) gachaPanel.SetActive(false);
        if (gachaResultPanel != null) gachaResultPanel.SetActive(false);
        if (homePanel != null) homePanel.SetActive(false);
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

        if (pauseButton != null && gameManager != null)
        {
            pauseButton.onClick.AddListener(() => gameManager.TogglePause());
        }

        if (resumeButton != null && gameManager != null)
        {
            resumeButton.onClick.AddListener(() => gameManager.TogglePause());
        }

        InitializeSkillDictionaryText();
        InitializePartySkillTexts();
        SetupPartyVisibility();
        UpdateAllUI();
    }

    void InitializePartySkillTexts()
    {
        if (partySkillTexts == null || gameManager == null || gameManager.skillDatabase == null) return;

        var charSkills = gameManager.skillDatabase.characterSkills;
        if (charSkills == null) return;

        for (int i = 0; i < partySkillTexts.Count; i++)
        {
            if (partySkillTexts[i] == null) continue;

            if (i < gameManager.partyMembers.Count)
            {
                string charName = gameManager.partyMembers[i].name;
                if (charSkills.ContainsKey(charName))
                {
                    partySkillTexts[i].text = string.Join("\n", charSkills[charName]);
                }
                else
                {
                    partySkillTexts[i].text = "";
                }
            }
            else
            {
                partySkillTexts[i].text = "";
            }
        }
    }

    /// <summary>
    /// 編成済みのキャラクター枚に応じて、パーティ枚のUIを表示/非表示にする
    /// </summary>
    public void SetupPartyVisibility()
    {
        if (gameManager == null || partyMemberAreas == null) return;

        int memberCount = gameManager.partyMembers.Count;

        for (int i = 0; i < partyMemberAreas.Count; i++)
        {
            if (partyMemberAreas[i] != null)
            {
                partyMemberAreas[i].SetActive(i < memberCount);
            }
        }
    }

    void InitializeSkillDictionaryText()
    {
        if (skillDictionaryText == null || gameManager == null || gameManager.skillDatabase == null) return;

        var charSkills = gameManager.skillDatabase.characterSkills;
        var allDesc = gameManager.skillDatabase.GetAllSkillDescriptions();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (charSkills != null)
        {
            // 編成中のキャラのスキルのみ表示
            foreach (var member in gameManager.partyMembers)
            {
                if (charSkills.ContainsKey(member.name))
                {
                    sb.AppendLine($"<color=#00FFFF>【{member.name}】</color>");
                    foreach (string skill in charSkills[member.name])
                    {
                        string desc = allDesc.ContainsKey(skill) ? allDesc[skill] : "固有スキル";
                        sb.AppendLine($"・<color=#FFFF00>{skill}</color> : {desc}");
                    }
                    sb.AppendLine();
                }
            }
        }

        skillDictionaryText.text = sb.ToString();
    }

    // ========================================
    // チュートリアルテキスト
    // ========================================

    /// <summary>
    /// 初回バトル時の攻撃チュートリアルを表示
    /// </summary>
    public void ShowAttackTutorial()
    {
        if (hasShownAttackTutorial || tutorialText == null) return;
        hasShownAttackTutorial = true;
        ShowTutorialMessage("「attack」とキーボードで打てば、攻撃できる！", 10f);
    }

    /// <summary>
    /// 初回ダメージ時の回復チュートリアルを表示
    /// </summary>
    public void ShowHealTutorial()
    {
        if (hasShownHealTutorial || tutorialText == null) return;
        hasShownHealTutorial = true;
        ShowTutorialMessage("「apple」とキーボードで打てば、回復できる！", 15f);
    }

    void ShowTutorialMessage(string message, float duration)
    {
        if (tutorialText == null) return;
        tutorialText.text = message;
        tutorialText.gameObject.SetActive(true);
        if (hideTutorialCoroutine != null) StopCoroutine(hideTutorialCoroutine);
        hideTutorialCoroutine = StartCoroutine(HideTutorialAfterDelay(duration));
    }

    System.Collections.IEnumerator HideTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (tutorialText != null) tutorialText.gameObject.SetActive(false);
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

    public void ShowVictory(int earnedCoins)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryCoinText != null)
            {
                victoryCoinText.text = $"HACK COINS +{earnedCoins}!";
            }
        }
    }

    // ========================================
    // ホーム画面UI関連
    // ========================================
    public void ShowHomePanel()
    {
        if (homePanel != null) homePanel.SetActive(true);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gachaPanel != null) gachaPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateHomeCoinText();
    }

    public void HideHomePanel()
    {
        if (homePanel != null) homePanel.SetActive(false);
    }

    public void UpdateHomeCoinText()
    {
        if (homeCoinText != null && PlayerDataManager.Instance != null)
        {
            homeCoinText.text = $"HACK COINS: {PlayerDataManager.Instance.HackCoins}";
        }
    }

    // ========================================
    // ガチャUI関連
    // ========================================
    public void ShowGachaPanel()
    {
        if (gachaPanel != null)
        {
            gachaPanel.SetActive(true);
            UpdateCoinText();
        }
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (homePanel != null) homePanel.SetActive(false);
    }

    public void HideGachaPanel()
    {
        if (gachaPanel != null) gachaPanel.SetActive(false);
        if (gachaResultPanel != null) gachaResultPanel.SetActive(false);
    }

    public void HideGachaResultPanel()
    {
        if (gachaResultPanel != null) gachaResultPanel.SetActive(false);
    }

    public void UpdateCoinText()
    {
        if (coinText != null && PlayerDataManager.Instance != null)
        {
            coinText.text = $"Coins: {PlayerDataManager.Instance.HackCoins}";
        }
    }

    public void ShowGachaResult(string message, string charaName = null)
    {
        if (gachaResultPanel != null) gachaResultPanel.SetActive(true);
        if (gachaResultText != null) gachaResultText.text = message;

        if (gachaResultImage != null)
        {
            if (!string.IsNullOrEmpty(charaName))
            {
                gachaResultImage.gameObject.SetActive(true);
                // リソースのパスからSpriteを取得（png対応）
                #if UNITY_EDITOR
                Sprite spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Chara/2/{charaName}2.png");
                if (spr == null) spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Chara/{charaName}.jpg");
                if (spr != null) gachaResultImage.sprite = spr;
                #endif
            }
            else
            {
                gachaResultImage.gameObject.SetActive(false);
            }
        }
    }

    // ========================================
    // ポーズ・スキル辞書UI
    // ========================================

    public void ToggleSkillDictionaryPanel(bool show)
    {
        if (skillDictionaryPanel != null)
        {
            skillDictionaryPanel.SetActive(show);
        }
    }

    public void UpdateSuggestText(List<string> suggestions)
    {
        if (suggestText == null) return;

        if (suggestions == null || suggestions.Count == 0)
        {
            suggestText.text = "Type a word...";
            suggestText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f); // 薄いグレー
        }
        else
        {
            suggestText.text = string.Join(" / ", suggestions);
            suggestText.color = Color.white;
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
        if (mainFont != null) tmp.font = mainFont;
        tmp.fontSize = Random.Range(56, 96);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.overflowMode = TextOverflowModes.Overflow;
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

    // ========================================
    // コンボ演出
    // ========================================

    public void ShowComboText(string text)
    {
        if (comboText != null)
        {
            StartCoroutine(ComboTextCoroutine(text));
        }
    }

    IEnumerator ComboTextCoroutine(string text)
    {
        comboText.text = text;
        comboText.gameObject.SetActive(true);

        RectTransform rect = comboText.GetComponent<RectTransform>();
        float duration = 1.0f;
        float elapsed = 0f;

        // 初期スケール
        rect.localScale = Vector3.one * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t <= 0.2f)
            {
                // ポップアップ（バウンスアウト）
                float subT = t / 0.2f;
                // オーバーシュートする計算を追加（シンプルに1.2倍まで拡大して1に戻る）
                float scale = Mathf.Lerp(0.5f, 1.2f, Mathf.Sin(subT * Mathf.PI / 2f));
                if (subT > 0.8f) scale = Mathf.Lerp(1.2f, 1.0f, (subT - 0.8f) / 0.2f);
                rect.localScale = Vector3.one * scale;
                comboText.color = new Color(1f, 0.8f, 0f, 1f); // 黄色/オレンジ系
            }
            else if (t > 0.7f)
            {
                // フェードアウト
                float subT = (t - 0.7f) / 0.3f;
                float alpha = Mathf.Lerp(1f, 0f, subT);
                comboText.color = new Color(1f, 0.8f, 0f, alpha);
            }

            yield return null;
        }

        comboText.gameObject.SetActive(false);
        rect.localScale = Vector3.one;
    }
}
