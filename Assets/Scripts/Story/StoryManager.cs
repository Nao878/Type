using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 会話シーンの進行とUI表示を管理するクラス
/// </summary>
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("Story UI References")]
    public GameObject storyPanel;
    public Image leftCharacterImage;
    public Image rightCharacterImage;
    public TMP_Text speakerNameText;
    public TMP_Text dialogText;
    public Button nextButton;
    public Button skipButton;
    
    [Header("Choices UI")]
    public GameObject choicesContainer; // GridLayoutGroupなどを持つ親
    public GameObject choiceButtonPrefab; // 選択肢ボタンのプレハブ

    [Header("State")]
    public bool isStoryActive = false;
    private StoryData currentStoryData;
    private int currentNodeIndex = 0;
    private Action onStoryComplete;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
        if (skipButton != null) skipButton.onClick.AddListener(SkipStory);
        
        HideStoryUI();
    }

    /// <summary>
    /// ストーリーの再生を開始する
    /// </summary>
    public void PlayStory(StoryData storyData, Action onComplete = null)
    {
        onStoryComplete = onComplete;

        if (storyData == null || storyData.nodes.Count == 0)
        {
            EndStory();
            return;
        }

        currentStoryData = storyData;
        currentNodeIndex = 0;
        isStoryActive = true;
        
        ShowStoryUI();
        DisplayCurrentNode();
    }

    /// <summary>
    /// 現在のノード（セリフや立ち絵）をUIに反映させる
    /// </summary>
    void DisplayCurrentNode()
    {
        if (currentNodeIndex >= currentStoryData.nodes.Count)
        {
            EndStory();
            return;
        }

        StoryNode node = currentStoryData.nodes[currentNodeIndex];

        // テキスト更新
        speakerNameText.text = node.speakerName;
        dialogText.text = node.dialogText;

        // 立ち絵更新
        if (node.leftCharacterImage != null)
        {
            leftCharacterImage.sprite = node.leftCharacterImage;
            leftCharacterImage.gameObject.SetActive(true);
        }
        else
        {
            leftCharacterImage.gameObject.SetActive(false);
        }
        
        if (node.rightCharacterImage != null)
        {
            rightCharacterImage.sprite = node.rightCharacterImage;
            rightCharacterImage.gameObject.SetActive(true);
        }
        else
        {
            rightCharacterImage.gameObject.SetActive(false);
        }

        // 選択肢の処理
        ClearChoices();
        if (node.choices != null && node.choices.Count > 0)
        {
            // 次へ進む全体ボタンを無効化し、選択肢を生成
            nextButton.gameObject.SetActive(false);
            choicesContainer.SetActive(true);

            foreach (var choice in node.choices)
            {
                CreateChoiceButton(choice);
            }
        }
        else
        {
            // 選択肢がない場合はクリック送りを有効化
            nextButton.gameObject.SetActive(true);
            choicesContainer.SetActive(false);
        }
    }

    void CreateChoiceButton(StoryChoice choice)
    {
        if (choiceButtonPrefab == null || choicesContainer == null) return;

        GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
        btnObj.SetActive(true);
        
        TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
        if (btnText != null) btnText.text = choice.choiceText;

        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => OnChoiceSelected(choice));
    }

    void ClearChoices()
    {
        if (choicesContainer == null) return;
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void OnChoiceSelected(StoryChoice choice)
    {
        if (choice.nextStoryData != null)
        {
            // 別のStoryDataへ分岐
            PlayStory(choice.nextStoryData);
        }
        else
        {
            // 分岐指定がない場合はそのまま次のノードへ
            currentNodeIndex++;
            DisplayCurrentNode();
        }
    }

    void OnNextClicked()
    {
        if (!isStoryActive) return;

        currentNodeIndex++;
        DisplayCurrentNode();
    }

    void SkipStory()
    {
        EndStory();
    }

    void EndStory()
    {
        isStoryActive = false;
        HideStoryUI();

        // 登録されたコールバックがあれば実行
        if (onStoryComplete != null)
        {
            Action callback = onStoryComplete;
            onStoryComplete = null;
            callback.Invoke();
        }
        else
        {
            // コールバックがない場合はデフォルトでGameManagerを通じてBattleへ移行する（後方互換）
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.Story)
            {
                GameManager.Instance.EndStoryTransitionToBattle();
            }
        }
    }

    void ShowStoryUI()
    {
        if (storyPanel != null) storyPanel.SetActive(true);
    }

    void HideStoryUI()
    {
        if (storyPanel != null) storyPanel.SetActive(false);
    }
}
