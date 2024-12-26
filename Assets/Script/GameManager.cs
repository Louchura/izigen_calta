using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Image[] handCards; // 手札のイラスト（複数）
    public Image problemCard; // 問題のイラスト
    public GameObject resultPanel; // 正解/不正解演出用パネル
    public Text resultText; // 演出用テキスト
    public GameObject choicePanel; // 選択肢UI（続ける・終了ボタン）
    public Text timerText; // 制限時間表示用テキスト

    private int correctIndex; // 正解の手札のインデックス
    private float timeLimit = 10f; // 制限時間（秒）
    private float remainingTime; // 残り時間
    private bool isTimeRunning = false; // タイマーの状態を管理

    void Start()
    {
        StartGame();
    }

    // ゲーム開始処理
    public void StartGame()
    {
        resultPanel.SetActive(false);
        choicePanel.SetActive(false);
        timerText.gameObject.SetActive(true);
        SetHandCards();
        SetProblemCard();
        StartTimer();
    }

    // 手札を設定
    void SetHandCards()
    {
        for (int i = 0; i < handCards.Length; i++)
        {
            handCards[i].sprite = GetRandomSprite();
            int index = i; // ローカルコピーを作成
            handCards[i].GetComponent<Button>().onClick.RemoveAllListeners();
            handCards[i].GetComponent<Button>().onClick.AddListener(() => OnCardClicked(index));
        }
    }

    // 問題カードを設定
    void SetProblemCard()
    {
        correctIndex = Random.Range(0, handCards.Length); // 正解の手札をランダムに決定
        problemCard.sprite = handCards[correctIndex].sprite; // 問題カードに正解のイラストを設定
    }

    // タイマー開始
    void StartTimer()
    {
        remainingTime = timeLimit;
        isTimeRunning = true;
    }

    void Update()
    {
        if (isTimeRunning)
        {
            remainingTime -= Time.deltaTime; // 残り時間を減少
            timerText.text = "残り時間: " + Mathf.Ceil(remainingTime).ToString() + "秒";

            if (remainingTime <= 0)
            {
                isTimeRunning = false;
                OnTimeUp();
            }
        }
    }

    // 時間切れ処理
    void OnTimeUp()
    {
        timerText.gameObject.SetActive(false);
        ShowResult(false); // 時間切れは不正解とみなす
    }

    // カードがクリックされたときの処理
    void OnCardClicked(int index)
    {
        if (!isTimeRunning) return; // タイマーが動いていない場合は無効

        isTimeRunning = false; // タイマーを停止
        timerText.gameObject.SetActive(false); // タイマーUIを非表示

        if (index == correctIndex)
        {
            ShowResult(true);
        }
        else
        {
            ShowResult(false);
        }
    }

    // 結果の表示
    void ShowResult(bool isCorrect)
    {
        resultPanel.SetActive(true);
        resultText.text = isCorrect ? "正解！" : "不正解！";
        Invoke(nameof(ShowChoicePanel), 2.0f); // 2秒後に選択肢を表示
    }

    // 選択肢を表示
    void ShowChoicePanel()
    {
        choicePanel.SetActive(true);
    }

    // ゲームを続ける
    public void ContinueGame()
    {
        StartGame();
    }

    // ゲームを終了する
    public void QuitGame()
    {
        Application.Quit();
    }

    // ランダムなイラストを取得する（仮）
    Sprite GetRandomSprite()
    {
        // ここでリソースからランダムにスプライトをロードする処理を書く
        return null;
    }
}