using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Image.fillAmount���g����HP�o�[���X�V����R���|�[�l���g
public class HpUIController : MonoBehaviour
{
    public Image hpImage; // �~�`Image

    // HP�̊����ōX�V(0.0�`1.0)
    public void UpdateHpBar(float ratio)
    {
        hpImage.fillAmount = ratio;
    }
}
