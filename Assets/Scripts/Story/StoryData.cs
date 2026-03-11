using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StoryChoice
{
    [Tooltip("選択肢のテキスト")]
    public string choiceText;
    
    [Tooltip("選択時に遷移する次のStoryData（nullならそのまま次のノードへ、または終了）")]
    public StoryData nextStoryData;
}

[System.Serializable]
public class StoryNode
{
    [Tooltip("発言者の名前")]
    public string speakerName;
    
    [TextArea(3, 5)]
    [Tooltip("セリフのテキスト")]
    public string dialogText;
    
    [Tooltip("左側に表示する立ち絵（nullなら非表示または前回のまま）")]
    public Sprite leftCharacterImage;
    
    [Tooltip("右側に表示する立ち絵（nullなら非表示または前回のまま）")]
    public Sprite rightCharacterImage;
    
    [Tooltip("このセリフの後に表示する選択肢（要素数0なら通常のクリック送り）")]
    public List<StoryChoice> choices = new List<StoryChoice>();
}

[CreateAssetMenu(fileName = "NewStoryData", menuName = "TypingRPG/Story Data")]
public class StoryData : ScriptableObject
{
    [Tooltip("このストーリー中の会話ノードリスト")]
    public List<StoryNode> nodes = new List<StoryNode>();
}
