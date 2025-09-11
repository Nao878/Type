using System.Collections.Generic;
using UnityEngine;

// 入力された文字に応じた効果を処理する
public class EffectHandler : MonoBehaviour
{
    public CharacterManager characterManager;

    public void ProcessWord(string word)
    {
        if (word.ToLower() == "apple") // 「apple」と入力されたら
        {
            // プレイヤー1（partyMembers[0]）のスキルとして回復
            Character player1 = characterManager.partyMembers.Count > 0 ? characterManager.partyMembers[0] : null;
            if (player1 != null && !player1.skillOnCooldown && player1.sp == player1.maxSp)
            {
                Character target = characterManager.GetLowestHPCharacter();
                if (target != null)
                {
                    target.Heal(20); // その味方のHPを20回復
                    characterManager.UpdateAllHpUI(); // 回復後にHP UI更新
                    player1.StartSkillCooldown(); // クールダウン開始＆SP0（他のキャラには影響しない）
                    characterManager.UpdateAllSpUI(); // SP UI更新
                }
            }
        }
    }
}
