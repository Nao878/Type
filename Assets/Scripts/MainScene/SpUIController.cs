using UnityEngine;
using UnityEngine.UI;

// SPバーを更新するコンポーネント
public class SpUIController : MonoBehaviour
{
    public Image spImage; // 円形Image

    // SPの割合で更新(0.0～1.0)
    public void UpdateSpBar(float ratio)
    {
        float clampedRatio = Mathf.Clamp01(ratio);
        spImage.fillAmount = clampedRatio;
        Debug.Log($"[SpUIController] UpdateSpBar: ratio={ratio}, clamped={clampedRatio}, fillAmount={spImage.fillAmount}");
    }
}
