using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class CardData
    {
        public int unitId; // CSVのunit_id
        public Sprite sprite; // 対応するイラスト
    }

    public List<CardData> cardDatabase; // CSVデータから読み込むカード情報
    public Image[] handCards; // 手札のイラスト
    public Image problemCard; // 問題のイラスト
    public GameObject resultPanel; // 正解/不正解演出用パネル
    public Text resultText; // 演出用テキスト
    public GameObject choicePanel; // 選択肢UI（続ける・終了ボタン）
    public Text timerText; // 制限時間表示用テキスト

    private int correctIndex; // 正解の手札のインデックス
    private float timeLimit = 10f; // 制限時間（秒）
    private float remainingTime; // 残り時間
    private bool isTimeRunning = false; // タイマーの状態を管理

    private List<CardData> handCardData; // 手札のカードデータ

    void Start()
    {
        LoadCSV(); // CSVのデータを読み込む
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

    // CSVデータを読み込む
    void LoadCSV()
{
    // Resourcesフォルダ内のCSVファイルを読み込む
    TextAsset csvFile = Resources.Load<TextAsset>("card_data"); // ファイル名（拡張子は不要）

    if (csvFile == null)
    {
        Debug.LogError("CSVファイルが見つかりません。Resourcesフォルダ内に配置してください。");
        return;
    }

    // CSVの内容を1行ごとに分割
    string[] lines = csvFile.text.Split('\n');

    cardDatabase = new List<CardData>(); // cardDatabaseを初期化

    for (int i = 1; i < lines.Length; i++) // ヘッダー行をスキップするために1から開始
    {
        string line = lines[i].Trim();

        // 空行をスキップ
        if (string.IsNullOrEmpty(line)) continue;

        // 行をカンマで分割
        string[] values = line.Split(',');

        if (values.Length < 2)
        {
            Debug.LogWarning($"行{i}のデータが不正です: {line}");
            continue;
        }

        // unit_idを取得
        if (!int.TryParse(values[0], out int unitId))
        {
            Debug.LogWarning($"行{i}のunit_idが不正です: {values[0]}");
            continue;
        }

        // スプライトを取得
        string spritePath = values[1];
        Sprite sprite = Resources.Load<Sprite>(spritePath);

        if (sprite == null)
        {
            Debug.LogWarning($"行{i}のスプライトが見つかりません: {spritePath}");
            continue;
        }

        // CardDataオブジェクトを作成し、リストに追加
        cardDatabase.Add(new CardData
        {
            unitId = unitId,
            sprite = sprite
        });
    }

    Debug.Log($"CSVから{cardDatabase.Count}枚のカードデータを読み込みました。");
}


    // 手札を設定
    void SetHandCards()
    {
        handCardData = new List<CardData>();
        List<CardData> shuffledCards = cardDatabase.OrderBy(x => Random.value).ToList();

        // 問題カードに一致する正解カードを決定
        CardData correctCard = shuffledCards[0];
        handCardData.Add(correctCard);

        // 残りの手札をランダムに選ぶ（正解と異なるunit_idを持つもの）
        foreach (var card in shuffledCards)
        {
            if (handCardData.Count >= handCards.Length) break;
            if (!handCardData.Contains(card) && card.unitId != correctCard.unitId)
            {
                handCardData.Add(card);
            }
        }

        // 手札のイラストをUIに反映
        for (int i = 0; i < handCards.Length; i++)
        {
            handCards[i].sprite = handCardData[i].sprite;
            int index = i; // ローカルコピーを作成
            handCards[i].GetComponent<Button>().onClick.RemoveAllListeners();
            handCards[i].GetComponent<Button>().onClick.AddListener(() => OnCardClicked(index));
        }
    }

    // 問題カードを設定
    void SetProblemCard()
    {
        correctIndex = Random.Range(0, handCardData.Count); // 正解の手札のインデックスをランダムに決定
        problemCard.sprite = handCardData[correctIndex].sprite; // 問題カードのイラストを設定
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
}
