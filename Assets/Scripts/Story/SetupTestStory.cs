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

        StoryNode node1 = new StoryNode();
        node1.speakerName = "System";
        node1.dialogText = "ワード・ハッカーズの世界へようこそ。\nタイピングで敵を倒せ！";
        data.nodes.Add(node1);

        StoryNode node2 = new StoryNode();
        node2.speakerName = "GlassMan";
        node2.dialogText = "俺のスペルは「apple」「cure」「glass」だ。\n回復と防御はお任せあれ。";
        data.nodes.Add(node2);
        
        StoryNode node3 = new StoryNode();
        node3.speakerName = "CatGirl";
        node3.dialogText = "私は「cat」で1秒間無敵よ！\n危ない時は任せて！";
        data.nodes.Add(node3);
        
        StoryNode node4 = new StoryNode();
        node4.speakerName = "System";
        node4.dialogText = "さあ、バトル開始だ！";
        data.nodes.Add(node4);

        gameManager.sampleStoryData = data;
    }
}
