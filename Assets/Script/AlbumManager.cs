using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

public class AlbumManager : MonoBehaviour
{
    [System.Serializable]
    public class UserData
    {
        public int highScore;
        public List<string> correctCards;
    }

    [System.Serializable]
    public class CardData
    {
        public string uniqueId;
        public string spritePath;
    }

    [System.Serializable]
    public class DescriptionData
    {
        public string uniqueId;
        public string description;
        public string videoUrl;
        public string videoTitle;
    }

    public GameObject cardPrefab;
    public Transform contentParent;
    public GameObject detailPanel;
    public Image detailImage;
    public Text detailText;
    public Button videoButton;
    public GameObject warningPanel;
    public Text warningText;
    public Button confirmButton;
    public Button cancelButton;

    private List<CardData> cardDatabase;
    private Dictionary<string, DescriptionData> descriptionDatabase;
    private UserData userData;
    private string currentVideoUrl;

    void Start()
    {
        LoadUserData();
        LoadCardData();
        LoadDescriptionData();
        PopulateAlbum();
        detailPanel.SetActive(false);
        warningPanel.SetActive(false);
    }

    // ✅ ユーザーデータのロード
    void LoadUserData()
    {
        string path = Path.Combine(Application.persistentDataPath, "user_data.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            userData = JsonConvert.DeserializeObject<UserData>(json);
        }
        else
        {
            userData = new UserData { highScore = 0, correctCards = new List<string>() };
        }
    }

    // ✅ カードデータのロード
    void LoadCardData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("unit_data");
        cardDatabase = new List<CardData>();

        foreach (string line in csvFile.text.Split('\n').Skip(1))
        {
            string[] values = line.Split(',');
            if (values.Length >= 2)
            {
                cardDatabase.Add(new CardData { uniqueId = values[0], spritePath = values[1] });
            }
        }
    }

    // ✅ 説明データのロード
    void LoadDescriptionData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("description_data");
        descriptionDatabase = new Dictionary<string, DescriptionData>();

        foreach (string line in csvFile.text.Split('\n').Skip(1))
        {
            string[] values = line.Split(',');
            if (values.Length >= 4)
            {
                descriptionDatabase[values[0]] = new DescriptionData
                {
                    uniqueId = values[0],
                    description = values[1],
                    videoUrl = values[2],
                    videoTitle = values[3]
                };
            }
        }
    }

    // ✅ アルバムを生成
    void PopulateAlbum()
    {
        foreach (var card in cardDatabase)
        {
            GameObject newCard = Instantiate(cardPrefab, contentParent);
            Image cardImage = newCard.transform.Find("Image").GetComponent<Image>();
            Button cardButton = newCard.GetComponent<Button>();
            GameObject overlay = newCard.transform.Find("Overlay").gameObject;
            Text lockText = newCard.transform.Find("LockText").GetComponent<Text>();

            Sprite sprite = Resources.Load<Sprite>(card.spritePath);
            cardImage.sprite = sprite;

            if (userData.correctCards.Contains(card.uniqueId))
            {
                overlay.SetActive(false);
                lockText.gameObject.SetActive(false);
                cardButton.onClick.AddListener(() => ShowDetail(card.uniqueId));
            }
            else
            {
                overlay.SetActive(true);
                lockText.gameObject.SetActive(true);
                cardButton.onClick.AddListener(() => ShowLockedMessage());
            }
        }
    }

    // ✅ 詳細パネルを表示
    void ShowDetail(string uniqueId)
    {
        if (descriptionDatabase.ContainsKey(uniqueId))
        {
            detailPanel.SetActive(true);
            detailImage.sprite = Resources.Load<Sprite>(cardDatabase.First(c => c.uniqueId == uniqueId).spritePath);
            detailText.text = descriptionDatabase[uniqueId].description;
            currentVideoUrl = descriptionDatabase[uniqueId].videoUrl;
            videoButton.onClick.RemoveAllListeners();
            videoButton.onClick.AddListener(() => ShowWarning(descriptionDatabase[uniqueId].videoTitle));
        }
    }

    // ✅ ロックされたカードのメッセージ
    void ShowLockedMessage()
    {
        Debug.Log("ゲームで正解すると詳細を表示できます");
    }

    // ✅ 外部リンク警告
    void ShowWarning(string videoTitle)
    {
        warningText.text = $"「{videoTitle}」を開きますか？";
        warningPanel.SetActive(true);

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => OpenVideo());

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => warningPanel.SetActive(false));
    }

    // ✅ YouTubeを開く
    void OpenVideo()
    {
        Application.OpenURL(currentVideoUrl);
        warningPanel.SetActive(false);
    }
}

