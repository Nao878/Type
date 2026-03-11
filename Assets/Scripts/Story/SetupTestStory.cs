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

        StoryData data = ScriptableObject.CreateInstance<StoryData>();

#if UNITY_EDITOR
        Sprite glassManSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Chara/GlassMan.jpg");
#else
        Sprite glassManSprite = null;
#endif

        StoryNode node1 = new StoryNode();
        node1.speakerName = "System";
        node1.dialogText = "警告。仮想キャンパスの異常なロックダウンを検知。管理AI『Shogun』が全権限を掌握しました。ログアウトは不可能です。";
        node1.leftCharacterImage = null;
        data.nodes.Add(node1);

        StoryNode node2 = new StoryNode();
        node2.speakerName = "GlassMan";
        node2.dialogText = "嘘だろ、ログアウトできない！？……いや、待てよ。ここは仮想空間だ。";
        node2.leftCharacterImage = glassManSprite;
        data.nodes.Add(node2);
        
        StoryNode node3 = new StoryNode();
        node3.speakerName = "GlassMan";
        node3.dialogText = "コンソールから直接コマンド（英単語）を打ち込めば、システムの物理法則を上書きできるかもしれない！俺の『apple』コマンドで修復プログラムを走らせる！";
        node3.leftCharacterImage = glassManSprite;
        data.nodes.Add(node3);
        
        StoryNode node4 = new StoryNode();
        node4.speakerName = "System";
        node4.dialogText = "敵性防壁プログラム、接近中。コンバットモードへ移行します。";
        node4.leftCharacterImage = null;
        data.nodes.Add(node4);

        gameManager.sampleStoryData = data;
    }
}
