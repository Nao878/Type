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
}
