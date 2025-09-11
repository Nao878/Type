using UnityEngine;
using UnityEngine.UI;

// SP�o�[���X�V����R���|�[�l���g
public class SpUIController : MonoBehaviour
{
    public Image spImage; // �~�`Image

    // SP�̊����ōX�V(0.0�`1.0)
    public void UpdateSpBar(float ratio)
    {
        float clampedRatio = Mathf.Clamp01(ratio);
        spImage.fillAmount = clampedRatio;
        Debug.Log($"[SpUIController] UpdateSpBar: ratio={ratio}, clamped={clampedRatio}, fillAmount={spImage.fillAmount}");
    }
}
