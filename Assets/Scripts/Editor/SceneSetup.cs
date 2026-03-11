using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Events;
#endif

/// <summary>
/// エディタ上でシーンを自動構築するセットアップスクリプト
/// </summary>
public class SceneSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("TypingRPG/Setup Scene")]
    public static void SetupScene()
    {
        // 既存のオブジェクトを削除（カメラとEventSystem以外）
        ClearScene();

        // GameManagerオブジェクト作成
        GameObject gameManagerObj = new GameObject("GameManager");
        GameManager gameManager = gameManagerObj.AddComponent<GameManager>();
        
        // テスト用ストーリーセットアップスクリプトを追加
        SetupTestStory setupTestStory = gameManagerObj.AddComponent<SetupTestStory>();
        setupTestStory.gameManager = gameManager;
        
        // Enemyオブジェクト作成
        GameObject enemyObj = new GameObject("Enemy");
        Enemy enemy = enemyObj.AddComponent<Enemy>();
        enemy.gameManager = gameManager;
        gameManager.enemy = enemy;

        // TypingControllerオブジェクト作成
        GameObject typingObj = new GameObject("TypingController");
        TypingController typingController = typingObj.AddComponent<TypingController>();
        gameManager.typingController = typingController;

        // SkillDatabaseオブジェクト作成
        GameObject skillDbObj = new GameObject("SkillDatabase");
        SkillDatabase skillDatabase = skillDbObj.AddComponent<SkillDatabase>();
        skillDatabase.gameManager = gameManager;
        skillDatabase.enemy = enemy;
        typingController.skillDatabase = skillDatabase;
        gameManager.skillDatabase = skillDatabase;
        typingController.uiManager = null; // uiManagerは後で設定
        
        // StoryManagerオブジェクト作成
        GameObject storyManagerObj = new GameObject("StoryManager");
        StoryManager storyManager = storyManagerObj.AddComponent<StoryManager>();

        // GachaManagerオブジェクト作成
        GameObject gachaManagerObj = new GameObject("GachaManager");
        GachaManager gachaManager = gachaManagerObj.AddComponent<GachaManager>();
        gachaManager.gameManager = gameManager;

        // FormationManagerオブジェクト作成
        GameObject formationManagerObj = new GameObject("FormationManager");
        FormationManager formationManager = formationManagerObj.AddComponent<FormationManager>();
        formationManager.gameManager = gameManager;

        // Canvas作成
        Canvas canvas = CreateCanvas();
        
        // UIManager作成
        GameObject uiManagerObj = new GameObject("UIManager");
        uiManagerObj.transform.SetParent(canvas.transform);
        UIManager uiManager = uiManagerObj.AddComponent<UIManager>();
        uiManager.gameManager = gameManager;
        uiManager.enemy = enemy;
        gameManager.uiManager = uiManager;
        skillDatabase.uiManager = uiManager;
        typingController.uiManager = uiManager;
        gachaManager.uiManager = uiManager;
        formationManager.uiManager = uiManager;

        // UI構築
        SetupUI(canvas, uiManager, typingController, gameManager, storyManager, gachaManager, formationManager);

        // シーンをdirtyにマークして保存を促す
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        // シーンを自動保存するか確認
        if (EditorUtility.DisplayDialog(
            "シーン保存",
            "シーンセットアップが完了しました。\nシーンを保存しますか？\n\n※保存しないとWebGLビルドに反映されません。",
            "保存する",
            "後で保存"))
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("シーンセットアップ完了＆保存しました！");
        }
        else
        {
            Debug.Log("シーンセットアップ完了！（未保存 - Ctrl+Sで保存してください）");
        }
    }

    static void ClearScene()
    {
        // アクティブなシーンのルートオブジェクトを取得して削除
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            if (obj == null) continue;
            
            // カメラとEventSystemは残す（子オブジェクトに含まれている場合も考慮）
            if (obj.GetComponent<Camera>() != null || obj.GetComponentInChildren<Camera>(true) != null) continue;
            if (obj.GetComponent<UnityEngine.EventSystems.EventSystem>() != null || obj.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>(true) != null) continue;
            
            DestroyImmediate(obj);
        }

        // カメラがなければ作成
        if (Camera.main == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
            camObj.AddComponent<AudioListener>();
        }

        // EventSystemがなければ作成
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    static Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        return canvas;
    }

    static TMP_FontAsset currentFontAsset;

    static void SetupUI(Canvas canvas, UIManager uiManager, TypingController typingController, GameManager gameManager, StoryManager storyManager, GachaManager gachaManager, FormationManager formationManager)
    {
        currentFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/NotoSansJP-Bold SDF.asset");
        if (uiManager != null) uiManager.mainFont = currentFontAsset;

        // 背景
        GameObject background = CreateImage(canvas.transform, "Background", new Vector2(0, 0), new Vector2(1920, 1080));
        background.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
        background.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        background.GetComponent<RectTransform>().anchorMax = Vector2.one;
        background.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // === 敵エリア（上部中央）===
        GameObject enemyArea = new GameObject("EnemyArea");
        enemyArea.transform.SetParent(canvas.transform);
        RectTransform enemyAreaRect = enemyArea.AddComponent<RectTransform>();
        enemyAreaRect.anchoredPosition = new Vector2(0, 400); // 300 -> 400
        enemyAreaRect.sizeDelta = new Vector2(400, 300);

        // 敵画像
        GameObject enemyImageObj = CreateImage(enemyArea.transform, "EnemyImage", Vector2.zero, new Vector2(280, 280));
        uiManager.enemyImage = enemyImageObj.GetComponent<Image>();
        
        // 敵画像のSprite読み込み試行
        Sprite enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Chara/_6ba5b67f-3389-42c9-b225-d3c277b32556.jpg");
        if (enemySprite != null)
        {
            uiManager.enemyImage.sprite = enemySprite;
        }

        // 敵HPバー背景
        GameObject enemyHPBg = CreateImage(enemyArea.transform, "EnemyHPBarBg", new Vector2(0, -130), new Vector2(400, 40));
        enemyHPBg.GetComponent<Image>().color = Color.gray;

        // 敵HPバー
        GameObject enemyHPBarObj = CreateImage(enemyArea.transform, "EnemyHPBar", new Vector2(0, -130), new Vector2(400, 40));
        Image enemyHPBar = enemyHPBarObj.GetComponent<Image>();
        enemyHPBar.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        enemyHPBar.color = Color.red;
        enemyHPBar.type = Image.Type.Filled;
        enemyHPBar.fillMethod = Image.FillMethod.Horizontal;
        uiManager.enemyHPBar = enemyHPBar;

        // 敵HPテキスト
        GameObject enemyHPTextObj = CreateText(enemyArea.transform, "EnemyHPText", new Vector2(0, -170), "50/50");
        uiManager.enemyHPText = enemyHPTextObj.GetComponent<TMP_Text>();

        // 状態異常アイコン
        uiManager.poisonEffectIcon = CreateImage(enemyArea.transform, "PoisonIcon", new Vector2(-80, -180), new Vector2(30, 30));
        uiManager.poisonEffectIcon.GetComponent<Image>().color = new Color(0.5f, 0f, 0.5f);
        
        uiManager.freezeEffectIcon = CreateImage(enemyArea.transform, "FreezeIcon", new Vector2(-40, -180), new Vector2(30, 30));
        uiManager.freezeEffectIcon.GetComponent<Image>().color = Color.cyan;
        
        uiManager.slowEffectIcon = CreateImage(enemyArea.transform, "SlowIcon", new Vector2(0, -180), new Vector2(30, 30));
        uiManager.slowEffectIcon.GetComponent<Image>().color = Color.yellow;

        // === 味方エリア（下部）===
        uiManager.partyMemberImages = new System.Collections.Generic.List<Image>();
        uiManager.partyHPBars = new System.Collections.Generic.List<Image>();
        uiManager.partyHPTexts = new System.Collections.Generic.List<TMP_Text>();
        uiManager.protectEffectIcons = new System.Collections.Generic.List<GameObject>();
        uiManager.targetHighlights = new System.Collections.Generic.List<Image>();
        uiManager.partySkillTexts = new System.Collections.Generic.List<TMP_Text>();
        uiManager.partyMemberAreas = new System.Collections.Generic.List<GameObject>();

        string[] characterImages = {
            "Assets/Images/Chara/2/GlassMan2.png",
            "Assets/Images/Chara/2/GentleMan2.png",
            "Assets/Images/Chara/2/CatGirl2.png",
            "Assets/Images/Chara/2/YellowGirl2.png"
        };

        for (int i = 0; i < 4; i++)
        {
            float xPos = -450 + i * 300;
            
            GameObject partyMemberArea = new GameObject($"PartyMember{i + 1}");
            partyMemberArea.transform.SetParent(canvas.transform);
            RectTransform partyRect = partyMemberArea.AddComponent<RectTransform>();
            partyRect.anchoredPosition = new Vector2(xPos, -200);
            partyRect.sizeDelta = new Vector2(250, 300);
            uiManager.partyMemberAreas.Add(partyMemberArea);

            // ターゲットハイライト
            GameObject highlight = CreateImage(partyMemberArea.transform, "TargetHighlight", Vector2.zero, new Vector2(210, 210));
            highlight.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.3f);
            uiManager.targetHighlights.Add(highlight.GetComponent<Image>());

            // キャラ画像（拡大）
            GameObject charImageObj = CreateImage(partyMemberArea.transform, "CharImage", Vector2.zero, new Vector2(200, 200));
            Image charImage = charImageObj.GetComponent<Image>();
            uiManager.partyMemberImages.Add(charImage);

            // Sprite読み込み
            Sprite charSprite = AssetDatabase.LoadAssetAtPath<Sprite>(characterImages[i]);
            if (charSprite != null)
            {
                charImage.sprite = charSprite;
            }

            // 無敵アイコン
            GameObject protectIcon = CreateImage(partyMemberArea.transform, "ProtectIcon", new Vector2(60, 60), new Vector2(40, 40));
            protectIcon.GetComponent<Image>().color = new Color(1f, 1f, 0f, 0.8f);
            uiManager.protectEffectIcons.Add(protectIcon);

            // HPバー背景
            GameObject hpBg = CreateImage(partyMemberArea.transform, "HPBarBg", new Vector2(0, -90), new Vector2(280, 30));
            hpBg.GetComponent<Image>().color = Color.gray;

            // HPバー
            GameObject hpBarObj = CreateImage(partyMemberArea.transform, "HPBar", new Vector2(0, -90), new Vector2(280, 30));
            Image hpBar = hpBarObj.GetComponent<Image>();
            hpBar.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            hpBar.color = Color.green;
            hpBar.type = Image.Type.Filled;
            hpBar.fillMethod = Image.FillMethod.Horizontal;
            uiManager.partyHPBars.Add(hpBar);

            // HPテキスト
            GameObject hpTextObj = CreateText(partyMemberArea.transform, "HPText", new Vector2(0, -125), "10/10");
            uiManager.partyHPTexts.Add(hpTextObj.GetComponent<TMP_Text>());

            // スキルカンペテキスト
            GameObject partySkillTextObj = CreateText(partyMemberArea.transform, "SkillText", new Vector2(0, -210), "");
            TMP_Text partySkillTmp = partySkillTextObj.GetComponent<TMP_Text>();
            partySkillTmp.fontSize = 24;
            partySkillTmp.alignment = TextAlignmentOptions.Top;
            RectTransform partySkillRect = partySkillTextObj.GetComponent<RectTransform>();
            partySkillRect.sizeDelta = new Vector2(280, 150);
            uiManager.partySkillTexts.Add(partySkillTmp);
        }

        // === タイピングエリア（中央下）===
        GameObject typingArea = new GameObject("TypingArea");
        typingArea.transform.SetParent(canvas.transform);
        RectTransform typingRect = typingArea.AddComponent<RectTransform>();
        typingRect.anchoredPosition = new Vector2(0, 0); // -100 -> 0
        typingRect.sizeDelta = new Vector2(1200, 200);

        // 入力表示背景
        GameObject inputBg = CreateImage(typingArea.transform, "InputBg", Vector2.zero, new Vector2(1000, 120));
        inputBg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // 現在入力テキスト
        GameObject inputTextObj = CreateText(typingArea.transform, "CurrentInputText", Vector2.zero, "");
        TMP_Text inputText = inputTextObj.GetComponent<TMP_Text>();
        inputText.fontSize = 72;
        inputText.alignment = TextAlignmentOptions.Center;
        uiManager.currentInputText = inputText;
        typingController.currentInputText = inputText;

        // サジェスト表示テキスト
        GameObject suggestTextObj = CreateText(typingArea.transform, "SuggestText", new Vector2(0, -75), "Type a word...");
        TMP_Text suggestText = suggestTextObj.GetComponent<TMP_Text>();
        suggestText.fontSize = 40;
        suggestText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f); // 薄いグレー初期色
        suggestText.alignment = TextAlignmentOptions.Center;
        uiManager.suggestText = suggestText;

        // スキル発動表示
        GameObject skillTextObj = CreateText(typingArea.transform, "SkillActivationText", new Vector2(0, 100), "");
        TMP_Text skillText = skillTextObj.GetComponent<TMP_Text>();
        skillText.fontSize = 56;
        skillText.color = Color.yellow;
        skillText.alignment = TextAlignmentOptions.Center;
        uiManager.skillActivationText = skillText;

        // タイピングパーティクルコンテナ（文字飛散演出用）
        GameObject particleContainer = new GameObject("TypingParticleContainer");
        particleContainer.transform.SetParent(typingArea.transform);
        RectTransform particleRect = particleContainer.AddComponent<RectTransform>();
        particleRect.anchoredPosition = new Vector2(0, 30);
        particleRect.sizeDelta = new Vector2(1000, 400);
        uiManager.typingParticleContainer = particleContainer.transform;

        // === バフ表示エリア（右上）===
        GameObject buffTextObj = CreateText(canvas.transform, "BuffTimerText", new Vector2(700, 450), "");
        TMP_Text buffText = buffTextObj.GetComponent<TMP_Text>();
        buffText.fontSize = 48;
        buffText.color = new Color(1f, 0.5f, 0f);
        uiManager.buffTimerText = buffText;

        GameObject speedBuffTextObj = CreateText(canvas.transform, "SpeedBuffTimerText", new Vector2(700, 390), "");
        TMP_Text speedBuffText = speedBuffTextObj.GetComponent<TMP_Text>();
        speedBuffText.fontSize = 48;
        speedBuffText.color = Color.cyan;
        uiManager.speedBuffTimerText = speedBuffText;

        // === ゲーム終了パネル ===
        // ゲームオーバーパネル
        GameObject gameOverPanel = CreateImage(canvas.transform, "GameOverPanel", Vector2.zero, new Vector2(800, 400));
        gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
        GameObject gameOverText = CreateText(gameOverPanel.transform, "GameOverText", Vector2.zero, "GAME OVER");
        gameOverText.GetComponent<TMP_Text>().fontSize = 144;
        gameOverText.GetComponent<TMP_Text>().color = Color.red;
        uiManager.gameOverPanel = gameOverPanel;

        // 勝利パネル
        GameObject victoryPanel = CreateImage(canvas.transform, "VictoryPanel", Vector2.zero, new Vector2(800, 500));
        victoryPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
        GameObject victoryText = CreateText(victoryPanel.transform, "VictoryText", new Vector2(0, 150), "VICTORY!");
        victoryText.GetComponent<TMP_Text>().fontSize = 120;
        victoryText.GetComponent<TMP_Text>().color = Color.green;
        
        GameObject vCoinTextObj = CreateText(victoryPanel.transform, "VictoryCoinText", new Vector2(0, 0), "HACK COINS +0!");
        TMP_Text vCoinTmp = vCoinTextObj.GetComponent<TMP_Text>();
        vCoinTmp.fontSize = 80;
        vCoinTmp.color = Color.yellow;
        uiManager.victoryCoinText = vCoinTmp;

        GameObject toHomeBtnObj = CreateButton(victoryPanel.transform, "ToHomeBtn", new Vector2(0, -150), new Vector2(400, 100), "HOME");
        Button toHomeBtn = toHomeBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(toHomeBtn.onClick, new UnityEngine.Events.UnityAction(gameManager.GoToHome));

        uiManager.victoryPanel = victoryPanel;

        // === HomePanelの作成 ===
        SetupHomeUI(canvas, uiManager, gameManager);

        // === Formation System Panel ===
        SetupFormationUI(canvas, formationManager);

        // === Gacha System Panel ===
        SetupGachaUI(canvas, uiManager, gachaManager);

        // === CRITICAL!!テキスト（believeスキルクリティカル演出用） ===
        GameObject criticalTextObj = CreateText(canvas.transform, "CriticalText", Vector2.zero, "");
        TMP_Text critText = criticalTextObj.GetComponent<TMP_Text>();
        critText.fontSize = 240;
        critText.color = new Color(1f, 0.1f, 0.1f, 1f);
        critText.alignment = TextAlignmentOptions.Center;
        critText.fontStyle = FontStyles.Bold;
        critText.enableWordWrapping = false;
        RectTransform critTextRect = criticalTextObj.GetComponent<RectTransform>();
        critTextRect.sizeDelta = new Vector2(1600, 400);
        criticalTextObj.SetActive(false);
        uiManager.criticalText = critText;

        // === 大技・発狂演出テキスト ===
        GameObject dangerTextObj = CreateText(enemyArea.transform, "DangerText", new Vector2(0, 100), "DANGER");
        TMP_Text dangerTmp = dangerTextObj.GetComponent<TMP_Text>();
        dangerTmp.fontSize = 96;
        dangerTmp.color = Color.red;
        dangerTmp.fontStyle = FontStyles.Bold;
        dangerTextObj.SetActive(false);
        uiManager.dangerTextObj = dangerTextObj;

        GameObject warningTextObj = CreateText(canvas.transform, "WarningText", new Vector2(0, 200), "WARNING! Type 'shield' to block!");
        TMP_Text warningTmp = warningTextObj.GetComponent<TMP_Text>();
        warningTmp.fontSize = 112;
        warningTmp.color = new Color(1f, 0.5f, 0f); // オレンジ
        warningTmp.fontStyle = FontStyles.Bold;
        warningTmp.enableWordWrapping = false;
        warningTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1600, 200);
        warningTextObj.SetActive(false);
        uiManager.warningTextObj = warningTextObj;

        GameObject blockedTextObj = CreateText(canvas.transform, "BlockedText", new Vector2(0, 50), "BLOCKED!");
        TMP_Text blockedTmp = blockedTextObj.GetComponent<TMP_Text>();
        blockedTmp.fontSize = 144;
        blockedTmp.color = Color.cyan;
        blockedTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
        blockedTextObj.SetActive(false);
        uiManager.blockedText = blockedTmp;

        // === コンボ表示テキスト ===
        GameObject comboTextObj = CreateText(canvas.transform, "ComboText", new Vector2(0, 100), "COMBO!");
        TMP_Text comboTmp = comboTextObj.GetComponent<TMP_Text>();
        comboTmp.fontSize = 128;
        comboTmp.color = new Color(1f, 0.8f, 0f); // 黄・オレンジ系
        comboTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
        comboTextObj.SetActive(false);
        uiManager.comboText = comboTmp;

        // === チュートリアルテキスト ===
        GameObject tutorialTextObj = CreateText(canvas.transform, "TutorialText", new Vector2(0, 150), "");
        TMP_Text tutorialTmp = tutorialTextObj.GetComponent<TMP_Text>();
        tutorialTmp.fontSize = 72;
        tutorialTmp.color = new Color(0.3f, 1f, 0.3f); // 緑色
        tutorialTmp.fontStyle = FontStyles.Bold;
        tutorialTmp.alignment = TextAlignmentOptions.Center;
        tutorialTmp.enableWordWrapping = false;
        tutorialTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1600, 200);
        tutorialTextObj.SetActive(false);
        uiManager.tutorialText = tutorialTmp;

        // === Pause / Spellbook ボタン ===
        GameObject spellbookObj = CreateButton(canvas.transform, "SpellbookButton", new Vector2(800, 480), new Vector2(160, 60), "Spellbook");
        Button spellbookButton = spellbookObj.GetComponent<Button>();
        uiManager.pauseButton = spellbookButton; // Assuming pauseButton is now spellbookButton

        // === StoryPanelの作成 ===
        SetupStoryUI(canvas, storyManager);

        // === Skill Dictionary Panel ===
        GameObject dictPanel = CreateImage(canvas.transform, "SkillDictionaryPanel", Vector2.zero, new Vector2(1920, 1080));
        dictPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.95f); // ほぼ真っ黒の半透明背景
        
        // タイトル
        GameObject dictTitle = CreateText(dictPanel.transform, "DictTitle", new Vector2(0, 450), "--- Spellbook ---");
        dictTitle.GetComponent<TMP_Text>().fontSize = 128;
        dictTitle.GetComponent<TMP_Text>().color = new Color(0.8f, 0.8f, 1f);

        // --- Scroll View ---
        GameObject scrollViewObj = new GameObject("Scroll View");
        scrollViewObj.transform.SetParent(dictPanel.transform);
        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchoredPosition = new Vector2(0, -60); // タイトルとボタンの下
        scrollViewRect.sizeDelta = new Vector2(1600, 800);
        UnityEngine.UI.ScrollRect scrollRect = scrollViewObj.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 50f;

        // Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        UnityEngine.UI.Image viewportImg = viewportObj.AddComponent<UnityEngine.UI.Image>(); // マスク用に必要
        viewportImg.color = new Color(1, 1, 1, 0.01f); // ほぼ透明
        viewportObj.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
        scrollRect.viewport = viewportRect;

        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0); // Fitterで伸びる
        contentRect.anchoredPosition = Vector2.zero;
        
        UnityEngine.UI.VerticalLayoutGroup vlg = contentObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;

        UnityEngine.UI.ContentSizeFitter csf = contentObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRect;

        // スクロールバー
        GameObject scrollbarObj = new GameObject("Scrollbar Vertical");
        scrollbarObj.transform.SetParent(scrollViewObj.transform);
        RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = new Vector2(1, 1);
        scrollbarRect.sizeDelta = new Vector2(20, 0);
        scrollbarRect.anchoredPosition = Vector2.zero;
        UnityEngine.UI.Image scrollbarBgImg = scrollbarObj.AddComponent<UnityEngine.UI.Image>();
        scrollbarBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        
        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbarObj.transform);
        RectTransform slidingAreaRect = slidingArea.AddComponent<RectTransform>();
        slidingAreaRect.anchorMin = Vector2.zero;
        slidingAreaRect.anchorMax = Vector2.one;
        slidingAreaRect.sizeDelta = Vector2.zero;
        slidingAreaRect.anchoredPosition = Vector2.zero;
        
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(slidingArea.transform);
        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.sizeDelta = new Vector2(-10, -10); // 余白
        handleRect.anchoredPosition = Vector2.zero;
        UnityEngine.UI.Image handleImg = handleObj.AddComponent<UnityEngine.UI.Image>();
        handleImg.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        UnityEngine.UI.Scrollbar scrollbar = scrollbarObj.AddComponent<UnityEngine.UI.Scrollbar>();
        scrollbar.direction = UnityEngine.UI.Scrollbar.Direction.BottomToTop;
        scrollbar.handleRect = handleRect;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        // スキルリストテキスト (Contentの子として作成)
        GameObject dictContent = CreateText(contentObj.transform, "DictContent", Vector2.zero, "");
        TMP_Text dictContentText = dictContent.GetComponent<TMP_Text>();
        dictContentText.fontSize = 56;
        dictContentText.alignment = TextAlignmentOptions.TopLeft; // 読みやすいように左上揃え
        dictContentText.lineSpacing = 15;
        // Text自体も縦に伸びるよう調整
        RectTransform dictContentRect = dictContent.GetComponent<RectTransform>();
        dictContentRect.sizeDelta = new Vector2(1500, 100);
        UnityEngine.UI.LayoutElement le = dictContent.AddComponent<UnityEngine.UI.LayoutElement>();
        le.flexibleWidth = 1;
        
        uiManager.skillDictionaryText = dictContentText;

        // Resume (Close) ボタン
        GameObject resumeBtnObj = CreateButton(dictPanel.transform, "ResumeButton", new Vector2(800, 450), new Vector2(240, 100), "Close");
        Button resumeBtn = resumeBtnObj.GetComponent<Button>();
        uiManager.resumeButton = resumeBtn;

        // パネルをUIManagerに割り当てて非表示にする
        uiManager.skillDictionaryPanel = dictPanel;
        dictPanel.SetActive(false);
    }

    static void SetupHomeUI(Canvas canvas, UIManager uiManager, GameManager gameManager)
    {
        if (uiManager == null || gameManager == null) return;

        // ホームパネル（全画面）
        GameObject homePanelObj = CreateImage(canvas.transform, "HomePanel", Vector2.zero, new Vector2(1920, 1080));
        homePanelObj.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.15f, 0.97f);
        uiManager.homePanel = homePanelObj;

        // タイトルテキスト
        GameObject titleObj = CreateText(homePanelObj.transform, "HomeTitle", new Vector2(0, 350), "WORD HACKERS");
        TMP_Text titleTmp = titleObj.GetComponent<TMP_Text>();
        titleTmp.fontSize = 120;
        titleTmp.color = Color.cyan;
        titleTmp.fontStyle = FontStyles.Bold;

        // サブタイトル
        GameObject subObj = CreateText(homePanelObj.transform, "HomeSubtitle", new Vector2(0, 250), "- Typing RPG -");
        TMP_Text subTmp = subObj.GetComponent<TMP_Text>();
        subTmp.fontSize = 56;
        subTmp.color = new Color(0.7f, 0.7f, 0.9f);

        // コイン表示
        GameObject coinObj = CreateText(homePanelObj.transform, "HomeCoinText", new Vector2(0, 130), "HACK COINS: 0");
        TMP_Text coinTmp = coinObj.GetComponent<TMP_Text>();
        coinTmp.fontSize = 64;
        coinTmp.color = Color.yellow;
        uiManager.homeCoinText = coinTmp;

        // BATTLE STARTボタン
        GameObject battleBtnObj = CreateButton(homePanelObj.transform, "BattleStartBtn", new Vector2(0, 0), new Vector2(600, 120), "BATTLE START");
        Button battleBtn = battleBtnObj.GetComponent<Button>();
        battleBtnObj.GetComponent<Image>().color = new Color(0.1f, 0.5f, 0.2f);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(battleBtn.onClick, new UnityEngine.Events.UnityAction(gameManager.GoToBattle));

        // DECODE（ガチャ）ボタン
        GameObject gachaBtnObj = CreateButton(homePanelObj.transform, "DecodeBtn", new Vector2(0, -160), new Vector2(600, 120), "DECODE (GACHA)");
        Button gachaBtn = gachaBtnObj.GetComponent<Button>();
        gachaBtnObj.GetComponent<Image>().color = new Color(0.4f, 0.1f, 0.5f);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(gachaBtn.onClick, new UnityEngine.Events.UnityAction(gameManager.GoToGacha));

        // FORMATION（編成）ボタン
        GameObject formBtnObj = CreateButton(homePanelObj.transform, "FormationBtn", new Vector2(0, -320), new Vector2(600, 120), "FORMATION");
        Button formBtn = formBtnObj.GetComponent<Button>();
        formBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.4f);
        if (FormationManager.Instance == null)
        {
            FormationManager fm = FindFirstObjectByType<FormationManager>();
            if (fm != null) UnityEditor.Events.UnityEventTools.AddPersistentListener(formBtn.onClick, new UnityEngine.Events.UnityAction(fm.OpenFormation));
        }

        homePanelObj.SetActive(false); // 初期は非表示
    }

    static void SetupFormationUI(Canvas canvas, FormationManager formationManager)
    {
        if (formationManager == null) return;

        // パネル本体（全画面）
        GameObject formationPanelObj = CreateImage(canvas.transform, "FormationPanel", Vector2.zero, new Vector2(1920, 1080));
        formationPanelObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        formationManager.formationPanel = formationPanelObj;

        // タイトル
        GameObject titleObj = CreateText(formationPanelObj.transform, "Title", new Vector2(0, 450), "PARTY FORMATION");
        TMP_Text titleTmp = titleObj.GetComponent<TMP_Text>();
        titleTmp.fontSize = 72;
        titleTmp.color = Color.cyan;
        titleTmp.fontStyle = FontStyles.Bold;

        // === 上部：パーティスロット（4つ） ===
        GameObject slotsContainerObj = new GameObject("SlotsContainer");
        slotsContainerObj.transform.SetParent(formationPanelObj.transform, false);
        RectTransform slotsRect = slotsContainerObj.AddComponent<RectTransform>();
        slotsRect.anchoredPosition = new Vector2(0, 200);
        slotsRect.sizeDelta = new Vector2(800, 200);
        
        HorizontalLayoutGroup slotsLayout = slotsContainerObj.AddComponent<HorizontalLayoutGroup>();
        slotsLayout.childAlignment = TextAnchor.MiddleCenter;
        slotsLayout.spacing = 50;
        slotsLayout.childControlWidth = false;
        slotsLayout.childControlHeight = false;

        formationManager.slotsContainer = slotsContainerObj.transform;

        for (int i = 0; i < 4; i++)
        {
            GameObject slotObj = CreateImage(slotsContainerObj.transform, $"Slot_{i}", Vector2.zero, new Vector2(140, 140));
            Image slotImg = slotObj.GetComponent<Image>();
            slotImg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

            FormationSlot fSlot = slotObj.AddComponent<FormationSlot>();
            fSlot.formationManager = formationManager;
            fSlot.slotImage = slotImg;
            formationManager.RegisterSlot(fSlot);
        }

        // === 下部：所持キャラリスト（Roster） ===
        GameObject rosterBgObj = CreateImage(formationPanelObj.transform, "RosterBG", new Vector2(0, -150), new Vector2(1400, 400));
        rosterBgObj.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.8f);

        GameObject rosterContainerObj = new GameObject("RosterContainer");
        rosterContainerObj.transform.SetParent(rosterBgObj.transform, false);
        RectTransform rosterRect = rosterContainerObj.AddComponent<RectTransform>();
        rosterRect.anchoredPosition = Vector2.zero;
        rosterRect.sizeDelta = new Vector2(1350, 350);

        GridLayoutGroup rosterGrid = rosterContainerObj.AddComponent<GridLayoutGroup>();
        rosterGrid.cellSize = new Vector2(120, 120);
        rosterGrid.spacing = new Vector2(30, 50);
        rosterGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        rosterGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        rosterGrid.childAlignment = TextAnchor.UpperLeft;

        formationManager.rosterContainer = rosterContainerObj.transform;

        // BACKボタン
        GameObject backBtnObj = CreateButton(formationPanelObj.transform, "BackButton", new Vector2(700, -400), new Vector2(300, 100), "BACK");
        Button backBtn = backBtnObj.GetComponent<Button>();
        backBtnObj.GetComponent<Image>().color = new Color(0.4f, 0.1f, 0.1f);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtn.onClick, new UnityEngine.Events.UnityAction(formationManager.CloseFormation));

        formationPanelObj.SetActive(false);
    }

    static void SetupStoryUI(Canvas canvas, StoryManager storyManager)
    {
        if (storyManager == null) return;

        // 大外のストーリーパネル（全画面）
        GameObject storyPanelObj = new GameObject("StoryPanel");
        storyPanelObj.transform.SetParent(canvas.transform);
        RectTransform panelRect = storyPanelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // 背景画像（暗い色）
        GameObject bgObj = CreateImage(storyPanelObj.transform, "StoryBackground", Vector2.zero, Vector2.zero);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // 立ち絵（左）— 正方形比率（1:1）
        GameObject leftChar = CreateImage(storyPanelObj.transform, "LeftCharacter", new Vector2(-500, 100), new Vector2(400, 400));
        leftChar.SetActive(false);
        storyManager.leftCharacterImage = leftChar.GetComponent<Image>();

        // 立ち絵（右）— 正方形比率（1:1）
        GameObject rightChar = CreateImage(storyPanelObj.transform, "RightCharacter", new Vector2(500, 100), new Vector2(400, 400));
        rightChar.SetActive(false); // 初期は非表示
        storyManager.rightCharacterImage = rightChar.GetComponent<Image>();

        // クリック送り用の全画面透明ボタン
        GameObject nextBtnObj = new GameObject("NextButton");
        nextBtnObj.transform.SetParent(storyPanelObj.transform);
        RectTransform nextRect = nextBtnObj.AddComponent<RectTransform>();
        nextRect.anchorMin = Vector2.zero;
        nextRect.anchorMax = Vector2.one;
        nextRect.sizeDelta = Vector2.zero;
        nextRect.anchoredPosition = Vector2.zero;
        Image nextImg = nextBtnObj.AddComponent<Image>();
        nextImg.color = new Color(0, 0, 0, 0); // 完全透明
        Button nextBtn = nextBtnObj.AddComponent<Button>();
        storyManager.nextButton = nextBtn;

        // メッセージウィンドウ（下部）
        GameObject msgWindow = CreateImage(storyPanelObj.transform, "MessageWindow", new Vector2(0, -350), new Vector2(1600, 300));
        msgWindow.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

        // 名前テキスト
        GameObject nameTextObj = CreateText(msgWindow.transform, "SpeakerNameText", new Vector2(-600, 100), "Speaker Name");
        TMP_Text nameTmp = nameTextObj.GetComponent<TMP_Text>();
        nameTmp.fontSize = 48;
        nameTmp.color = Color.cyan;
        nameTmp.alignment = TextAlignmentOptions.TopLeft;
        nameTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 100);
        storyManager.speakerNameText = nameTmp;

        // セリフテキスト
        GameObject dialogTextObj = CreateText(msgWindow.transform, "DialogText", new Vector2(0, -20), "Dialog goes here...");
        TMP_Text dialogTmp = dialogTextObj.GetComponent<TMP_Text>();
        dialogTmp.fontSize = 40;
        dialogTmp.alignment = TextAlignmentOptions.TopLeft;
        dialogTmp.enableWordWrapping = true;
        dialogTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1500, 200);
        storyManager.dialogText = dialogTmp;

        // 選択肢コンテナ（画面中央）
        GameObject choicesContainer = new GameObject("ChoicesContainer");
        choicesContainer.transform.SetParent(storyPanelObj.transform);
        RectTransform choicesRect = choicesContainer.AddComponent<RectTransform>();
        choicesRect.anchoredPosition = new Vector2(0, 0);
        choicesRect.sizeDelta = new Vector2(600, 400);
        UnityEngine.UI.VerticalLayoutGroup vlg = choicesContainer.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 20;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        storyManager.choicesContainer = choicesContainer;
        choicesContainer.SetActive(false);

        // 選択肢ボタンのプレハブを作成（とりあえずPrefabではなく非アクティブなオブジェクトとして持たせておく）
        GameObject choicePrefab = CreateButton(choicesContainer.transform, "ChoicePrefab", Vector2.zero, new Vector2(500, 80), "Choice");
        choicePrefab.SetActive(false);
        storyManager.choiceButtonPrefab = choicePrefab;

        // スキップボタン（右上）
        GameObject skipBtnObj = CreateButton(storyPanelObj.transform, "SkipButton", new Vector2(800, 450), new Vector2(160, 60), "Skip");
        Button skipBtn = skipBtnObj.GetComponent<Button>();
        storyManager.skipButton = skipBtn;

        // マネージャーとパネルをリンク
        storyManager.storyPanel = storyPanelObj;
        storyPanelObj.SetActive(false); // 初期は非表示
    }

    static void SetupGachaUI(Canvas canvas, UIManager uiManager, GachaManager gachaManager)
    {
        if (uiManager == null || gachaManager == null) return;

        // 大外のガチャパネル
        GameObject gachaPanelObj = CreateImage(canvas.transform, "GachaPanel", Vector2.zero, new Vector2(1920, 1080));
        gachaPanelObj.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        uiManager.gachaPanel = gachaPanelObj;

        // タイトル
        GameObject titleObj = CreateText(gachaPanelObj.transform, "GachaTitle", new Vector2(0, 400), "--- CHARACTER GACHA ---");
        titleObj.GetComponent<TMP_Text>().fontSize = 100;
        titleObj.GetComponent<TMP_Text>().color = Color.cyan;

        // 現在のコイン数
        GameObject coinTextObj = CreateText(gachaPanelObj.transform, "CoinText", new Vector2(0, 250), "Coins: 0");
        TMP_Text coinTmp = coinTextObj.GetComponent<TMP_Text>();
        coinTmp.fontSize = 80;
        coinTmp.color = Color.yellow;
        uiManager.coinText = coinTmp;

        // キャラを引くボタン
        GameObject drawBtnObj = CreateButton(gachaPanelObj.transform, "DrawButton", new Vector2(0, 50), new Vector2(500, 150), $"1 PULL (Cost: {gachaManager.GachaCost})");
        Button drawBtn = drawBtnObj.GetComponent<Button>();

        // 戻る（ホームへ）ボタン
        GameObject closeBtnObj = CreateButton(gachaPanelObj.transform, "CloseButton", new Vector2(0, -150), new Vector2(400, 100), "BACK");
        Button closeBtn = closeBtnObj.GetComponent<Button>();

        // === ガチャ結果パネル ===
        GameObject resultPanelObj = CreateImage(gachaPanelObj.transform, "GachaResultPanel", Vector2.zero, new Vector2(1200, 800));
        resultPanelObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.95f);
        uiManager.gachaResultPanel = resultPanelObj;

        // 結果メッセージ
        GameObject resTextObj = CreateText(resultPanelObj.transform, "ResultText", new Vector2(0, 300), "JOINED!");
        TMP_Text resTmp = resTextObj.GetComponent<TMP_Text>();
        resTmp.fontSize = 90;
        resTmp.color = Color.magenta;
        uiManager.gachaResultText = resTmp;

        // キャラ画像
        GameObject resImageObj = CreateImage(resultPanelObj.transform, "ResultImage", new Vector2(0, -50), new Vector2(400, 600));
        uiManager.gachaResultImage = resImageObj.GetComponent<Image>();

        // 結果を閉じるボタン
        GameObject resCloseBtnObj = CreateButton(resultPanelObj.transform, "ResultCloseButton", new Vector2(0, -300), new Vector2(300, 80), "OK");
        Button resCloseBtn = resCloseBtnObj.GetComponent<Button>();

        // イベント設定（Editor再生時にリセットされる可能性があるため、UIManager側でも対応できるように作っているが、一応AddListenerしておく）
        UnityEditor.Events.UnityEventTools.AddPersistentListener(drawBtn.onClick, new UnityEngine.Events.UnityAction(gachaManager.DrawGacha));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, new UnityEngine.Events.UnityAction(gachaManager.CloseGacha));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(resCloseBtn.onClick, new UnityEngine.Events.UnityAction(uiManager.HideGachaResultPanel));
    }

    static GameObject CreateImage(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = obj.AddComponent<Image>();
        image.color = Color.white;

        return obj;
    }

    static GameObject CreateText(Transform parent, string name, Vector2 position, string text)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(800, 100);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        if (currentFontAsset != null) tmp.font = currentFontAsset;
        tmp.fontSize = 48;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.overflowMode = TextOverflowModes.Overflow;

        return obj;
    }

    static GameObject CreateButton(Transform parent, string name, Vector2 position, Vector2 size, string text)
    {
        // 1. Buttonオブジェクト作成（背景画像付き）
        GameObject obj = CreateImage(parent, name, position, size);
        Image img = obj.GetComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f); // グレーのボタン背景

        // 2. Buttonコンポーネント追加
        Button btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;

        // 3. テキスト子オブジェクト作成
        GameObject textObj = CreateText(obj.transform, "Text", Vector2.zero, text);
        TMP_Text tmp = textObj.GetComponent<TMP_Text>();
        tmp.fontSize = 48;
        tmp.color = Color.white;

        return obj;
    }
#endif
}
