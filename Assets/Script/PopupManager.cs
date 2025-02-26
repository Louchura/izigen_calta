using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel; // ポップアップのパネル
    public Button openPopupButton; // ポップアップを開くボタン
    public Button closePopupButton; // ポップアップを閉じるボタン

    void Start()
    {
        // ポップアップは最初非表示
        popupPanel.SetActive(false);

        // ボタンにイベントを登録
        openPopupButton.onClick.AddListener(OpenPopup);
        closePopupButton.onClick.AddListener(ClosePopup);
    }

    // ポップアップを開く
    public void OpenPopup()
    {
        popupPanel.SetActive(true);
    }

    // ポップアップを閉じる
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}
