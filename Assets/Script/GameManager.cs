using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO; // JSONファイルの入出力用
using Newtonsoft.Json; // JSONを扱うためのライブラリ

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class CardData
    {
        public int unitId; // CSVのunit_id
        public string uniqueId;//CSVのunique_ID
        public Sprite sprite; // 対応するイラスト
        public Rect cropRect;
    }

    public class UserData
    {
        public int highScore; // ハイスコア
        public List<string> correctCards; // 正解したカードのunique_idリスト
    }

    private const string FILE_PATH = "user_data.json"; // ユーザーデータファイルのパス
    private UserData userData; // ユーザーデータ


    public List<CardData> cardDatabase; // CSVデータから読み込むカード情報
    public Image[] handCards; // 手札のイラスト
    public Image problemCard; // 問題のイラスト
    public GameObject resultPanel; // 正解/不正解演出用パネル
    public Text judgeText; // 演出用テキスト
    public GameObject choicePanel; // 選択肢UI（続ける・終了ボタン）
    public Text timerText; // 制限時間表示用テキスト
    public Text currentScoreText; //現在のスコアを表示するテキスト
    public Text currentHighScore; //ハイスコアを表示するテキスト
    public Text currentScoreResult; //リザルト画面に現在のスコアを表示するテキスト
    public Text resultText; //リザルト演出で表示するテキスト
    public Button continueButton; // 続けるボタン
    public Button quitButton; // 終了ボタン
    public GameObject pausePanel; // ポーズ用パネル
    public Button pauseButton; // ポーズボタン
    public Button resumeButton; // 続行ボタン
    public Button quitToTitleButton; // タイトルに戻るボタン

    private int correctIndex; // 正解の手札のインデックス
    private float timeLimit = 5f; // 制限時間（秒）
    private float remainingTime; // 残り時間
    private bool isTimeRunning = false; // タイマーの状態を管理

    private List<CardData> handCardData; // 手札のカードデータ
    private int currentScore = 0; // 現在のスコア
    private int highScore = 0; // ハイスコア
    private int gameCount = 0; // 3回セットのカウント
    private int comboCount = 0; // 連続正解数

    private bool isDebugMode = false; // デバッグモードの状態
    private int debugCardIndex = 0; // デバッグで表示するカードのインデックス


    void Start()
    {
        LoadUserData(); 
        LoadCSV(); // 引数を指定しない場合、全てのカードを読み込みますわ
        StartGame();
        // ボタンのリスナーを設定
        continueButton.onClick.AddListener(ContinueGame);
        quitButton.onClick.AddListener(QuitGame);

        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
        quitToTitleButton.onClick.AddListener(ReturnToTitle);

        // 選択肢パネルを非表示にする
        choicePanel.SetActive(false);
        // ポーズパネルを非表示
        pausePanel.SetActive(false);
    }

    // ユーザーデータをロード（なければ新規作成）
    void LoadUserData()
    {
        string path = Path.Combine(Application.persistentDataPath, FILE_PATH);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            userData = JsonConvert.DeserializeObject<UserData>(json);

            if (userData == null)
            {
                Debug.LogError("userData の JSON 読み込みに失敗しました。新規データを作成します。");
                userData = new UserData { highScore = 0, correctCards = new List<string>() };
                SaveUserData();
            }
            else if (userData.correctCards == null)
            {
                userData.correctCards = new List<string>();
            }
        }
        else
        {
            // 初期データ作成
            userData = new UserData
            {
                highScore = 0,
                correctCards = new List<string>()
            };
            SaveUserData();
        }
    }

    // ユーザーデータを保存
    void SaveUserData()
    {
        string path = Path.Combine(Application.persistentDataPath, FILE_PATH);
        string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
        File.WriteAllText(path, json);
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

    // ▼▼▼ こちらを改修いたしましたわ ▼▼▼
    // CSVデータを読み込む (特定のunique_idを指定できるように)
    void LoadCSV(string targetUniqueId = null)
    {
        TextAsset csvFile = Resources.Load<TextAsset>("unit_data");

        if (csvFile == null)
        {
            Debug.LogError("CSVファイルが見つかりません。Resourcesフォルダ内に配置してください。");
            return;
        }

        cardDatabase = new List<CardData>(); // データベースを初期化しますの

        string[] lines = csvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (values.Length < 7)
            {
                // Debug.LogWarning($"行{i}のデータが不完全です: {line}");
                continue;
            }
            
            string uniqueId = values[2]; // unique_IDを取得

            // IDが指定されている場合は一致するか確認し、指定がなければ全て読み込みますわ
            if (string.IsNullOrEmpty(targetUniqueId) || uniqueId == targetUniqueId)
            {
                if (!int.TryParse(values[0], out int unitId)) continue;

                string spritePath = values[1];
                Sprite sprite = Resources.Load<Sprite>(spritePath);
                if (sprite == null) continue;

                float cropX = float.Parse(values[3]);
                float cropY = float.Parse(values[4]);
                float cropWidth = float.Parse(values[5]);
                float cropHeight = float.Parse(values[6]);

                cardDatabase.Add(new CardData
                {
                    unitId = unitId,
                    uniqueId = uniqueId,
                    sprite = sprite,
                    cropRect = new Rect(cropX, cropY, cropWidth, cropHeight)
                });

                // 特定のIDを探している場合、見つけたらループを抜けて効率化しますの
                if (!string.IsNullOrEmpty(targetUniqueId))
                {
                    break;
                }
            }
        }
        Debug.Log($"CSVから{cardDatabase.Count}枚のカードデータを読み込みました。");
    }
    // ▲▲▲ ここまで ▲▲▲


    // 手札を設定
    void SetHandCards()
    {
        if (cardDatabase == null || cardDatabase.Count == 0)
        {
            Debug.LogError("カードデータベースが空ですわ！ LoadCSVが正しく実行されたかご確認くださいまし。");
            return;
        }

        handCardData = new List<CardData>();
        List<CardData> shuffledCards = cardDatabase.OrderBy(x => Random.value).ToList();
        
        CardData correctCard = shuffledCards[0];
        handCardData.Add(correctCard);

        foreach (var card in shuffledCards)
        {
            if (handCardData.Count >= handCards.Length) break;
            if (!handCardData.Any(c => c.unitId == card.unitId))
            {
                handCardData.Add(card);
            }
        }

        if (handCardData.Count < handCards.Length)
        {
            Debug.LogError($"手札が不足しています！ 必要: {handCards.Length}, 現在: {handCardData.Count}");
        }

        for (int i = 0; i < handCards.Length; i++)
        {
            if (i >= handCardData.Count) break; 
            if (handCardData[i] != null)
            {
                handCards[i].sprite = handCardData[i].sprite;
                int index = i;
                handCards[i].GetComponent<Button>().onClick.RemoveAllListeners();
                handCards[i].GetComponent<Button>().onClick.AddListener(() => OnCardClicked(index));
            }
        }
    }

    // 問題カードを設定
    void SetProblemCard()
    {
        correctIndex = Random.Range(0, handCardData.Count);
        CardData correctCard = handCardData[correctIndex];

        List<CardData> sameUnitIdCards = cardDatabase
            .Where(card => card.unitId == correctCard.unitId)
            .ToList();

        if (sameUnitIdCards.Count == 0)
        {
            problemCard.sprite = CreateCroppedSprite(correctCard.sprite, correctCard.cropRect);
            return;
        }

        CardData selectedCard = null;
        foreach (var card in sameUnitIdCards)
        {
            if (card.uniqueId != correctCard.uniqueId)
            {
                selectedCard = card;
                break;
            }
        }

        if (selectedCard != null)
        {
            problemCard.sprite = CreateCroppedSprite(selectedCard.sprite, selectedCard.cropRect);
        }
        else
        {
            problemCard.sprite = CreateCroppedSprite(correctCard.sprite, correctCard.cropRect);
        }
    }

    // タイマー開始
    void StartTimer()
    {
        remainingTime = timeLimit;
        isTimeRunning = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            isDebugMode = !isDebugMode;
            if (isDebugMode)
            {
                isTimeRunning = false; 
                resultPanel.SetActive(false);
                choicePanel.SetActive(false);
                pausePanel.SetActive(false);
                
                foreach (var cardImage in handCards) { cardImage.gameObject.SetActive(false); }
                timerText.gameObject.SetActive(false);
                currentScoreText.gameObject.SetActive(false);

                Debug.LogWarning("--- デバッグモードを開始します ---");
                ShowDebugCard();
            }
            else
            {
                Debug.LogWarning("--- デバッグモードを終了します ---");
                 foreach (var cardImage in handCards) { cardImage.gameObject.SetActive(true); }
                currentScoreText.gameObject.SetActive(true);
                StartGame();
            }
        }

        if (isDebugMode)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                debugCardIndex++;
                if (debugCardIndex >= cardDatabase.Count) { debugCardIndex = 0; }
                ShowDebugCard();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                debugCardIndex--;
                if (debugCardIndex < 0) { debugCardIndex = cardDatabase.Count - 1; }
                ShowDebugCard();
            }
        }
        else if (isTimeRunning)
        {
            remainingTime -= Time.deltaTime;
            timerText.text = Mathf.Ceil(remainingTime).ToString() + "秒";

            if (remainingTime <= 0)
            {
                isTimeRunning = false;
                OnTimeUp();
            }
        }
    }

    private Sprite CreateCroppedSprite(Sprite originalSprite, Rect cropRect)
    {
        Texture2D originalTexture = originalSprite.texture;
        Rect spriteRect = originalSprite.textureRect;

        Rect actualCrop = new Rect(spriteRect.x + cropRect.x, spriteRect.y + cropRect.y, cropRect.width, cropRect.height);

        Texture2D croppedTexture = new Texture2D((int)actualCrop.width, (int)actualCrop.height);
        Color[] pixels = originalTexture.GetPixels((int)actualCrop.x, (int)actualCrop.y, (int)actualCrop.width, (int)actualCrop.height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return Sprite.Create(croppedTexture, new Rect(0, 0, croppedTexture.width, croppedTexture.height), new Vector2(0.5f, 0.5f));
    }


    void OnTimeUp()
    {
        isTimeRunning = false;
        timerText.gameObject.SetActive(false); 
        Debug.Log("時間切れ！");
        ShowChoicePanel();
    }

    void UpdateScoreUI()
    {
        if (currentScoreText != null) { currentScoreText.text = "スコア:" + currentScore.ToString(); }
    }

    void OnCardClicked(int index)
    {
        if (!isTimeRunning || isDebugMode) return; 

        if (handCardData == null || handCardData.Count <= index || index < 0) return;

        isTimeRunning = false; 
        timerText.gameObject.SetActive(false); 

        if (index == correctIndex)
        {
            comboCount++;
            int bonus = (comboCount > 1) ? 500 : 0; 
            currentScore += 1000 + bonus;

            if (userData == null) return;
            
            string uniqueId = handCardData[index].uniqueId;
            if (!userData.correctCards.Contains(uniqueId)) 
            {
                userData.correctCards.Add(uniqueId);
            }
            SaveUserData();
           
            UpdateScoreUI();
            ShowResult(true);
        }
        else
        {
            comboCount = 0;
            ShowResult(false);
        }
    }

    void ShowResult(bool isCorrect)
    {
        resultPanel.SetActive(true);
        judgeText.text = isCorrect ? "正解！" : "不正解！";
        Invoke(nameof(EndRound), 2.0f); 
    }
    
    void EndRound()
    {
        gameCount++;
        if (gameCount >= 10)
        {
            if (currentScore > userData.highScore)
            {
                userData.highScore = currentScore;
                SaveUserData();
            }
            gameCount = 0;
            ShowChoicePanel();
        }
        else
        {
            StartGame();
        }
        UpdateScoreUI();
    }

    void ShowChoicePanel()
    {
        choicePanel.SetActive(true);
        currentHighScore.text = "これまでのハイスコア:" + userData.highScore.ToString();
        currentScoreResult.text = "今回のスコア:" + currentScore.ToString();
        if (currentScore > userData.highScore)
        {
            resultText.text = "ハイスコア更新！";
        }
        else
        {
            resultText.text = "ハイスコア更新失敗・・・";
        }
    }

    void ShowDebugCard()
    {
        if (cardDatabase == null || cardDatabase.Count == 0) return;
        
        CardData cardToShow = cardDatabase[debugCardIndex];
        problemCard.sprite = CreateCroppedSprite(cardToShow.sprite, cardToShow.cropRect);
        Debug.Log($"[デバッグ表示中] Index: {debugCardIndex}, UniqueID: {cardToShow.uniqueId}, UnitID: {cardToShow.unitId}");
    }
    
    public void ContinueGame()
    {
        choicePanel.SetActive(false); 
        currentScore = 0;
        UpdateScoreUI();
        StartGame();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("ゲーム終了"); 
    }
    
    public void PauseGame()
    {
        if (isDebugMode) return; 
        isTimeRunning = false; 
        pausePanel.SetActive(true);
    }
    
    public void ResumeGame()
    {
        isTimeRunning = true;
        pausePanel.SetActive(false);
    }

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("OutGame");
    }
}
