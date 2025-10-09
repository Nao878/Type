using UnityEngine;

[CreateAssetMenu(fileName = "SkillSettings", menuName = "Game/SkillSettings")]
public class SkillSettings : ScriptableObject
{
    [Header("Apple Skill Settings")]
    public int appleHealAmount = 20;
    [Header("Poison Skill Settings")]
    public int poisonDamage = 1;
    public float poisonInterval = 1f;
    public float poisonDuration = 5f;
    [Header("Stop Skill Settings")]
    public float stopDelayTime = 2f;
    [Header("Debuff Skill Settings")]
    public float debuffDuration = 3f;
    public int debuffDamageMultiplier = 2;
}
