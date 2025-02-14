using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 指定したシーンをロードする
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ゲームを終了する
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("ゲームを終了しました");
    }
}