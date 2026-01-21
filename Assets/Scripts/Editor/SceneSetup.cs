using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
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

        // UI構築
        SetupUI(canvas, uiManager, typingController);

        Debug.Log("シーンセットアップ完了！");
    }

    static void ClearScene()
    {
        // シーン内の全オブジェクトを取得
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            // カメラとEventSystemは残す
            if (obj.GetComponent<Camera>() != null) continue;
            if (obj.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) continue;
            if (obj.transform.parent != null) continue; // 子オブジェクトはスキップ
            
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

    static void SetupUI(Canvas canvas, UIManager uiManager, TypingController typingController)
    {
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
        enemyAreaRect.anchoredPosition = new Vector2(0, 300);
        enemyAreaRect.sizeDelta = new Vector2(400, 300);

        // 敵画像
        GameObject enemyImageObj = CreateImage(enemyArea.transform, "EnemyImage", Vector2.zero, new Vector2(200, 200));
        uiManager.enemyImage = enemyImageObj.GetComponent<Image>();
        
        // 敵画像のSprite読み込み試行
        Sprite enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Chara/_6ba5b67f-3389-42c9-b225-d3c277b32556.jpg");
        if (enemySprite != null)
        {
            uiManager.enemyImage.sprite = enemySprite;
        }

        // 敵HPバー背景
        GameObject enemyHPBg = CreateImage(enemyArea.transform, "EnemyHPBarBg", new Vector2(0, -130), new Vector2(200, 20));
        enemyHPBg.GetComponent<Image>().color = Color.gray;

        // 敵HPバー
        GameObject enemyHPBarObj = CreateImage(enemyArea.transform, "EnemyHPBar", new Vector2(0, -130), new Vector2(200, 20));
        Image enemyHPBar = enemyHPBarObj.GetComponent<Image>();
        enemyHPBar.color = Color.red;
        enemyHPBar.type = Image.Type.Filled;
        enemyHPBar.fillMethod = Image.FillMethod.Horizontal;
        uiManager.enemyHPBar = enemyHPBar;

        // 敵HPテキスト
        GameObject enemyHPTextObj = CreateText(enemyArea.transform, "EnemyHPText", new Vector2(0, -155), "50/50");
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

        string[] characterImages = {
            "Assets/Images/Chara/GlassMan.jpg",
            "Assets/Images/Chara/Gentleman.jpg",
            "Assets/Images/Chara/CatGirl.jpg",
            "Assets/Images/Chara/YellowGirl.jpg"
        };

        for (int i = 0; i < 4; i++)
        {
            float xPos = -450 + i * 300;
            
            GameObject partyMemberArea = new GameObject($"PartyMember{i + 1}");
            partyMemberArea.transform.SetParent(canvas.transform);
            RectTransform partyRect = partyMemberArea.AddComponent<RectTransform>();
            partyRect.anchoredPosition = new Vector2(xPos, -300);
            partyRect.sizeDelta = new Vector2(200, 250);

            // ターゲットハイライト
            GameObject highlight = CreateImage(partyMemberArea.transform, "TargetHighlight", Vector2.zero, new Vector2(160, 160));
            highlight.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.3f);
            uiManager.targetHighlights.Add(highlight.GetComponent<Image>());

            // キャラ画像
            GameObject charImageObj = CreateImage(partyMemberArea.transform, "CharImage", Vector2.zero, new Vector2(150, 150));
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
            GameObject hpBg = CreateImage(partyMemberArea.transform, "HPBarBg", new Vector2(0, -90), new Vector2(140, 15));
            hpBg.GetComponent<Image>().color = Color.gray;

            // HPバー
            GameObject hpBarObj = CreateImage(partyMemberArea.transform, "HPBar", new Vector2(0, -90), new Vector2(140, 15));
            Image hpBar = hpBarObj.GetComponent<Image>();
            hpBar.color = Color.green;
            hpBar.type = Image.Type.Filled;
            hpBar.fillMethod = Image.FillMethod.Horizontal;
            uiManager.partyHPBars.Add(hpBar);

            // HPテキスト
            GameObject hpTextObj = CreateText(partyMemberArea.transform, "HPText", new Vector2(0, -110), "10/10");
            uiManager.partyHPTexts.Add(hpTextObj.GetComponent<TMP_Text>());
        }

        // === タイピングエリア（中央下）===
        GameObject typingArea = new GameObject("TypingArea");
        typingArea.transform.SetParent(canvas.transform);
        RectTransform typingRect = typingArea.AddComponent<RectTransform>();
        typingRect.anchoredPosition = new Vector2(0, -100);
        typingRect.sizeDelta = new Vector2(600, 100);

        // 入力表示背景
        GameObject inputBg = CreateImage(typingArea.transform, "InputBg", Vector2.zero, new Vector2(500, 60));
        inputBg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // 現在入力テキスト
        GameObject inputTextObj = CreateText(typingArea.transform, "CurrentInputText", Vector2.zero, "");
        TMP_Text inputText = inputTextObj.GetComponent<TMP_Text>();
        inputText.fontSize = 36;
        inputText.alignment = TextAlignmentOptions.Center;
        uiManager.currentInputText = inputText;
        typingController.currentInputText = inputText;

        // スキル発動表示
        GameObject skillTextObj = CreateText(typingArea.transform, "SkillActivationText", new Vector2(0, 50), "");
        TMP_Text skillText = skillTextObj.GetComponent<TMP_Text>();
        skillText.fontSize = 28;
        skillText.color = Color.yellow;
        skillText.alignment = TextAlignmentOptions.Center;
        uiManager.skillActivationText = skillText;

        // === バフ表示エリア（右上）===
        GameObject buffTextObj = CreateText(canvas.transform, "BuffTimerText", new Vector2(800, 450), "");
        TMP_Text buffText = buffTextObj.GetComponent<TMP_Text>();
        buffText.fontSize = 24;
        buffText.color = new Color(1f, 0.5f, 0f);
        uiManager.buffTimerText = buffText;

        GameObject speedBuffTextObj = CreateText(canvas.transform, "SpeedBuffTimerText", new Vector2(800, 420), "");
        TMP_Text speedBuffText = speedBuffTextObj.GetComponent<TMP_Text>();
        speedBuffText.fontSize = 24;
        speedBuffText.color = Color.cyan;
        uiManager.speedBuffTimerText = speedBuffText;

        // === ゲーム終了パネル ===
        // ゲームオーバーパネル
        GameObject gameOverPanel = CreateImage(canvas.transform, "GameOverPanel", Vector2.zero, new Vector2(600, 300));
        gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
        GameObject gameOverText = CreateText(gameOverPanel.transform, "GameOverText", Vector2.zero, "GAME OVER");
        gameOverText.GetComponent<TMP_Text>().fontSize = 72;
        gameOverText.GetComponent<TMP_Text>().color = Color.red;
        uiManager.gameOverPanel = gameOverPanel;

        // 勝利パネル
        GameObject victoryPanel = CreateImage(canvas.transform, "VictoryPanel", Vector2.zero, new Vector2(600, 300));
        victoryPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
        GameObject victoryText = CreateText(victoryPanel.transform, "VictoryText", Vector2.zero, "VICTORY!");
        victoryText.GetComponent<TMP_Text>().fontSize = 72;
        victoryText.GetComponent<TMP_Text>().color = Color.green;
        uiManager.victoryPanel = victoryPanel;
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
        rect.sizeDelta = new Vector2(400, 50);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return obj;
    }
#endif
}
