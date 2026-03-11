using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// ドラッグ＆ドロップ式パーティ編成画面を管理するクラス
/// </summary>
public class FormationManager : MonoBehaviour
{
    public static FormationManager Instance { get; private set; }

    [Header("UI参照")]
    public GameObject formationPanel;
    public Transform slotsContainer;    // 上部の4スロット親
    public Transform rosterContainer;   // 下部の所持キャラリスト親
    public GameObject draggableIconPrefab; // テンプレート（動的生成用）

    [Header("参照")]
    public GameManager gameManager;
    public UIManager uiManager;

    // 内部データ
    private List<FormationSlot> partySlots = new List<FormationSlot>();
    private List<DraggableIcon> allIcons = new List<DraggableIcon>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 編成画面を開く
    /// </summary>
    public void OpenFormation()
    {
        if (formationPanel != null) formationPanel.SetActive(true);
        RefreshFormationUI();
    }

    /// <summary>
    /// 編成画面を閉じてデータを保存
    /// </summary>
    public void CloseFormation()
    {
        SaveFormation();
        if (formationPanel != null) formationPanel.SetActive(false);

        if (gameManager != null)
        {
            gameManager.GoToHome();
        }
    }

    /// <summary>
    /// 編成UIを再構築する
    /// </summary>
    public void RefreshFormationUI()
    {
        // 既存アイコンをクリア
        foreach (var icon in allIcons)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        allIcons.Clear();

        if (PlayerDataManager.Instance == null) return;

        List<string> unlocked = PlayerDataManager.Instance.UnlockedCharacters;
        List<string> currentFormation = PlayerDataManager.Instance.PartyFormation;

        // スロットをクリア
        foreach (var slot in partySlots)
        {
            slot.ClearSlot();
        }

        // 現在の編成をスロットに配置
        for (int i = 0; i < currentFormation.Count && i < partySlots.Count; i++)
        {
            string charName = currentFormation[i];
            if (!string.IsNullOrEmpty(charName) && unlocked.Contains(charName))
            {
                DraggableIcon icon = CreateCharacterIcon(charName, partySlots[i].transform);
                partySlots[i].SetCharacter(icon);
            }
        }

        // 編成に入っていないキャラを下部リストに配置
        foreach (string charName in unlocked)
        {
            if (!currentFormation.Contains(charName))
            {
                CreateCharacterIcon(charName, rosterContainer);
            }
        }
    }

    DraggableIcon CreateCharacterIcon(string charName, Transform parent)
    {
        GameObject iconObj = new GameObject($"Icon_{charName}");
        iconObj.transform.SetParent(parent, false);

        RectTransform rect = iconObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 120);

        Image img = iconObj.AddComponent<Image>();
        img.color = Color.white;

        // 画像読み込み
        #if UNITY_EDITOR
        Sprite spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Chara/2/{charName}2.png");
        if (spr == null) spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Images/Chara/{charName}.jpg");
        if (spr != null) img.sprite = spr;
        #endif

        // 名前テキスト
        GameObject nameObj = new GameObject("NameLabel");
        nameObj.transform.SetParent(iconObj.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchoredPosition = new Vector2(0, -70);
        nameRect.sizeDelta = new Vector2(140, 30);
        TMP_Text nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        nameTmp.text = charName;
        nameTmp.fontSize = 24;
        nameTmp.color = Color.white;
        nameTmp.alignment = TextAlignmentOptions.Center;

        // CanvasGroupでドラッグ中の透過
        CanvasGroup cg = iconObj.AddComponent<CanvasGroup>();

        DraggableIcon draggable = iconObj.AddComponent<DraggableIcon>();
        draggable.characterName = charName;
        draggable.formationManager = this;

        allIcons.Add(draggable);
        return draggable;
    }

    /// <summary>
    /// 現在のスロット状態をPlayerDataManagerに保存
    /// </summary>
    public void SaveFormation()
    {
        if (PlayerDataManager.Instance == null) return;

        List<string> formation = new List<string>();
        foreach (var slot in partySlots)
        {
            if (slot.currentIcon != null)
            {
                formation.Add(slot.currentIcon.characterName);
            }
        }

        PlayerDataManager.Instance.SavePartyFormation(formation);
    }

    /// <summary>
    /// アイコンがスロット外にドロップされた場合、下部リストへ戻す
    /// </summary>
    public void ReturnToRoster(DraggableIcon icon)
    {
        if (icon == null || rosterContainer == null) return;
        icon.transform.SetParent(rosterContainer, false);

        // 元のスロットから削除
        foreach (var slot in partySlots)
        {
            if (slot.currentIcon == icon)
            {
                slot.ClearSlot();
                break;
            }
        }
    }

    public void RegisterSlot(FormationSlot slot)
    {
        if (!partySlots.Contains(slot))
        {
            partySlots.Add(slot);
        }
    }
}

/// <summary>
/// ドラッグ可能なキャラクターアイコン
/// </summary>
public class DraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string characterName;
    public FormationManager formationManager;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas rootCanvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // ルートCanvasの子にして最前面に
        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // マウス位置に追従
        if (rootCanvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                eventData.position,
                rootCanvas.worldCamera,
                out localPoint
            );
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // ドロップ先がスロットでなければ下部リストに戻す
        if (eventData.pointerEnter == null ||
            eventData.pointerEnter.GetComponent<FormationSlot>() == null)
        {
            formationManager?.ReturnToRoster(this);
        }
    }
}

/// <summary>
/// パーティ編成のスロット（ドロップ先）
/// </summary>
public class FormationSlot : MonoBehaviour, IDropHandler
{
    public DraggableIcon currentIcon;
    public FormationManager formationManager;
    public Image slotImage; // スロット背景

    public void OnDrop(PointerEventData eventData)
    {
        DraggableIcon droppedIcon = eventData.pointerDrag?.GetComponent<DraggableIcon>();
        if (droppedIcon == null) return;

        // 既にこのスロットにキャラがいる場合は入れ替え
        if (currentIcon != null && currentIcon != droppedIcon)
        {
            // ドロップされたアイコンの元スロットを探す
            FormationSlot sourceSlot = null;
            if (formationManager != null)
            {
                // 全スロットから元のスロットを探す
                foreach (Transform child in formationManager.slotsContainer)
                {
                    FormationSlot s = child.GetComponent<FormationSlot>();
                    if (s != null && s.currentIcon == droppedIcon)
                    {
                        sourceSlot = s;
                        break;
                    }
                }
            }

            // 既存アイコンを元のスロットまたはリストへ移動
            if (sourceSlot != null)
            {
                sourceSlot.SetCharacter(currentIcon);
            }
            else
            {
                // ドラッグ元がリストだった場合は、既存キャラをリストへ
                formationManager?.ReturnToRoster(currentIcon);
            }
        }
        else
        {
            // 元のスロットをクリア
            if (formationManager != null)
            {
                foreach (Transform child in formationManager.slotsContainer)
                {
                    FormationSlot s = child.GetComponent<FormationSlot>();
                    if (s != null && s != this && s.currentIcon == droppedIcon)
                    {
                        s.ClearSlot();
                        break;
                    }
                }
            }
        }

        SetCharacter(droppedIcon);
    }

    public void SetCharacter(DraggableIcon icon)
    {
        currentIcon = icon;
        if (icon != null)
        {
            icon.transform.SetParent(transform, false);
            icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    public void ClearSlot()
    {
        currentIcon = null;
    }
}
