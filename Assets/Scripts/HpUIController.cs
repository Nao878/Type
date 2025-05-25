using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Image.fillAmountを使ってHPバーを更新するコンポーネント
public class HpUIController : MonoBehaviour
{
    public Image hpImage; // 円形Image

    // HPの割合で更新(0.0～1.0)
    public void UpdateHpBar(float ratio)
    {
        hpImage.fillAmount = ratio;
    }
}
