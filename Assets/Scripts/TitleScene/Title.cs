using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    // ボタンから呼び出す用
    public void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
