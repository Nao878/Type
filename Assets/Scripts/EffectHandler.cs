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
            // 最もHPの低い味方キャラを探す
            Character target = characterManager.GetLowestHPCharacter();

            if (target != null) // その味方キャラが居たら処理
            {
                target.Heal(20); // その味方のHPを20回復
                characterManager.UpdateAllHpUI(); // 回復後にUI更新
            }
        }
    }
}
