using UnityEngine;

/// <summary>
/// テスト用のStoryDataを動的に生成し、GameManagerに設定する
/// </summary>
public class SetupTestStory : MonoBehaviour
{
    public GameManager gameManager;

    void Start()
    {
        if (gameManager == null) return;

#if UNITY_EDITOR
        Sprite glassManSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Chara/2/GlassMan2.png");
        if (glassManSprite == null) glassManSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Chara/GlassMan.jpg");
#else
        Sprite glassManSprite = null;
#endif

        // ストーリー1：ゲーム開始直後
        StoryData story1 = ScriptableObject.CreateInstance<StoryData>();
        
        story1.nodes.Add(new StoryNode {
            speakerName = "System",
            dialogText = "警告。仮想キャンパスの防壁AI『Shogun』が暴走。全生徒のログアウト権限を凍結。",
            leftCharacterImage = null
        });
        story1.nodes.Add(new StoryNode {
            speakerName = "GlassMan",
            dialogText = "閉じ込められた！？…いや、ここは仮想空間だ。コンソールからコマンド（英単語）を打ち込めばシステムを書き換えられる！",
            leftCharacterImage = glassManSprite
        });
        story1.nodes.Add(new StoryNode {
            speakerName = "System",
            dialogText = "敵性プログラム接近。コンバットモードへ移行します。",
            leftCharacterImage = null
        });

        // ストーリー2：初回クリア時
        StoryData story2 = ScriptableObject.CreateInstance<StoryData>();

        story2.nodes.Add(new StoryNode {
            speakerName = "GlassMan",
            dialogText = "ふう、なんとか撃退できた。ここは安全領域（セーフゾーン）として使えそうだ。",
            leftCharacterImage = glassManSprite
        });
        story2.nodes.Add(new StoryNode {
            speakerName = "System",
            dialogText = "戦闘報酬として『ハックコイン』の暗号データを取得しました。",
            leftCharacterImage = null
        });
        story2.nodes.Add(new StoryNode {
            speakerName = "GlassMan",
            dialogText = "このコイン…ただのデータじゃない。他のみんなのアクセス権限の欠片だ！",
            leftCharacterImage = glassManSprite
        });
        story2.nodes.Add(new StoryNode {
            speakerName = "GlassMan",
            dialogText = "ホーム画面の『DECODE (ガチャ)』機能を使えば、このコインを消費して他の生徒たちの暗号化を解き、ここにサルベージ（救出）できるはずだ！",
            leftCharacterImage = glassManSprite
        });
        story2.nodes.Add(new StoryNode {
            speakerName = "GlassMan",
            dialogText = "頼む、みんなを救出してくれ！そして『FORMATION (編成)』でパーティを組んで、Shogunの中枢を目指そう！",
            leftCharacterImage = glassManSprite
        });

        gameManager.tutorialStory1 = story1;
        gameManager.tutorialStory2 = story2;
        gameManager.sampleStoryData = story1; // 後方互換フォールバック
    }
}
