using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Image[] handCards;//手札のイラスト（複数）
    public Image problemCard;//問題のイラスト
    public GameObject resultPanel;//正解・不正解演出用パネル
    public Text resultText;//演出用テキスト
    public GameObject choicePanel;//選択肢UI（続ける・終了ボタン）
    public Text timerText;//制限時間表示用テキスト

    private int correctIndex;//正解の手札のインデックス
    private float timeLimit=10f;//制限時間（秒）
    private float remainingTime;//残り時間
    private bool isTimeRunning=false;//タイマーの状態を管理
    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    //ゲーム開始処理
    public void StargGame(){
        resultPanel.setActive(false);
        choicePanel.setActive(false);
        timerText.GameObject.SetActive(true);
        SetHandCards();
        SetProblemCard();
        StartTimer();
    }

    //手札を設定
    void SetHandCards(){
        for(int i=0;i<handCards.Length;i++){
            //ランダムにイラストを設定（実際はリソースからロード）
            handCards{i}.sprite=GetRandomSprtite();
            int index=i;//ローカルコピーを作成
            handCards[i].GetComponent<Button>().onClick.RemoveAllListeners();
            handCards[i].GetComponent<Button>().onClick.AddListener(()=>OnCardClicked(index));

        }

    }

    //問題カードを設定
    void SetProblemCard(){
        correctIndex=Random.Range(0,handCards.Lentgh);//正解の手札をランダムに決定
        problemCard.sprite=handCards[correctIndex].sprite;//問題カードに正解のイラストを設定
    }

    //タイマー開始
    void StartTimer(){
        remainingTime=timeLimit;
        isTimeRunning=true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimeRunning){
            remainingTime -=Time.deltaTime;//残り時間を減少
            timerText.text="残り時間："+Mathf.Ceil(remainingTime).ToString()+"秒";

            if(remainingTime<=0){
                isTimeRunning=false;
                OnTimeUp();
            }
        }
        
    }

    //時間切れ処理
    void OnTimeUp(){
        timerText.GameObject.SetActive(false);
        ShowResult(false);//時間切れは不正解とみなす
    }
    //次はカードがクリックされた時の処理、から
}
