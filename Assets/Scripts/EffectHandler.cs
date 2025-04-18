using System.Collections.Generic;
using UnityEngine;

public class EffectHandler : MonoBehaviour
{
    public CharacterManager characterManager;

    public void ProcessWord(string word)
    {
        if (word.ToLower() == "apple")
        {
            Character target = characterManager.GetLowestHPCharacter();
            if (target != null)
            {
                target.Heal(20);
            }
            Debug.Log("‰ñ•œ‚µ‚½");
        }
    }
}
