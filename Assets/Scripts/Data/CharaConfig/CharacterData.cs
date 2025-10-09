using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public int maxHp;
    public int maxSp;
    // 必要に応じて他のステータスも追加
}
