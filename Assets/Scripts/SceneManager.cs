using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    [Min(100)]
    public int ShuffleCount = 100;
    List<Card.Data> cards;
    public Card CardPrefab;
    public GameObject Dealer;
    public GameObject Player;
    public GameObject BetsInputDialog;
    public InputField BetsInput;
    public Text BetsText;
    public Text PointText;
    public Button BetsInputOKButton;
    public Text ResultText;
    public float WaitResultSeconds = 2;
    public Text GoalPointText;
    public int goalPoint = 40;

    public int StartPoint = 20;
    int currentPoint;
    int currentBets;

    public enum Action
    {
        WaitAction = 0,
        Hit = 1,
        Stand = 2,
    }
    Action CurrentAction = Action.WaitAction;
    public void SetAction(int action)
    {
        CurrentAction = (Action)action;
    }

    private void Awake()
    {
        BetsInput.onValidateInput = BetsInputOnValidateInput;
        // onValidateInputは文字を入力する関数
        BetsInput.onValueChanged.AddListener(BetsInputOnValueChanged);
        // onValueChanged にBetsInputOnValueChangedを加える
        GoalPointText.text = goalPoint.ToString();
    }

    char BetsInputOnValidateInput(string text, int startIndex, char addedChar)
    {
        if(!char.IsDigit(addedChar)) return '\0';
        return addedChar;
    }

    void BetsInputOnValueChanged(string text)
    {
        BetsInputOKButton.interactable = false;
        // ボタンを押せないようにする
        if(int.TryParse(BetsInput.text,out var bets))
        // BetsInputをtextにしてbetsに代入する
        {
            if(0<bets && bets<=currentPoint)
            {
                BetsInputOKButton.interactable = true;
                // ボタンを押せるようにする
            }
        }
    }

    void InitCards()
    {
        cards = new List<Card.Data>(13*4);
        var marks = new List<Card.Mark>()
        {
            Card.Mark.Heart,
            Card.Mark.Diamond,
            Card.Mark.Spade,
            Card.Mark.Crub,
        };

        foreach(var mark in marks)
        {
            for(var num=1; num<=13;++num)
            {
                var card = new Card.Data()
                {
                    Mark = mark,
                    Number = num,
                };
                cards.Add(card);
            }
        }
        ShuffleCards();
    }

    void ShuffleCards()
    {
        var random = new System.Random();
        for(var i=0; i<ShuffleCount; ++i)
        {
            var index = random.Next(cards.Count);
            var index2 = random.Next(cards.Count);

            var tmp = cards[index];
            cards[index] = cards[index2];
            cards[index2] = tmp;
        }
    }

    Card.Data DealCard()
    {
        if(cards.Count <= 0) return null;

        var card = cards[0];
        cards.Remove(card);
        return card;
    }

    void DealCards()
    {
        foreach(Transform card in Dealer.transform)
        {
            Object.Destroy(card.gameObject);
        }

        foreach (Transform card in Player.transform)
        {
            Object.Destroy(card.gameObject);
        }

        var holeCardObj = Object.Instantiate(CardPrefab,Dealer.transform);
        // オブジェクトを生成する関数
        holeCardObj.IsLarge = holeCardObj.Number ==1;
        var holeCard = DealCard();
        holeCardObj.SetCard(holeCard.Number,holeCard.Mark,true);

        var upCardObj = Object.Instantiate(CardPrefab,Dealer.transform);
        upCardObj.IsLarge = upCardObj.Number ==1;
        var upCard = DealCard();
        upCardObj.SetCard(upCard.Number,upCard.Mark,false);

        for(var i=0;i<2;++i)
        {
            var cardObj = Object.Instantiate(CardPrefab,Player.transform);
            var card = DealCard();
            cardObj.SetCard(card.Number,card.Mark,false);
        }
    }

    Coroutine _gameLoopCoroutine;
    private void Start()
    {
        _gameLoopCoroutine = StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        currentPoint = StartPoint;
        BetsText.text = "0";
        PointText.text = currentPoint.ToString();
        ResultText.gameObject.SetActive(false);
        while(true)
        {
            InitCards();
            yield return null;
            do
            {
                BetsInputDialog.SetActive(true);
                yield return new WaitWhile(()=> BetsInputDialog.activeSelf);
                if(int.TryParse(BetsInput.text,out var bets))
                {
                    if(0<bets && bets <= currentPoint)
                    {
                        currentBets = bets;
                        break;
                    }
                }
            }while(true);

            BetsInputDialog.SetActive(false);
            BetsText.text = currentBets.ToString();

            DealCards();

            bool WaitAction = true;
            bool doWin = false;
            do
            {
                CurrentAction = Action.WaitAction;
                yield return new WaitWhile(() => CurrentAction == Action.WaitAction);

                switch (CurrentAction)
                {
                    case Action.Hit:
                        PlayerDealCard();
                        WaitAction = true;
                        if(!CheckPlayerCard())
                        {
                            WaitAction = false;
                            doWin = false;
                        }
                        break;
                    case Action.Stand:
                        WaitAction = false;
                        doWin = StandAction();
                        break;
                    default:
                        WaitAction = true;
                        throw new System.Exception("知らない行動を取ろうとしています。");
                }
            }while(WaitAction);

            ResultText.gameObject.SetActive(true);
            if(doWin)
            {
                currentPoint += currentBets;
                ResultText.text = "Win!! + " + currentBets;
            }
            else
            {
                currentPoint -= currentBets;
                ResultText.text = "Lose... - " + currentBets;
            }
            PointText.text = currentPoint.ToString();

            yield return new WaitForSeconds(WaitResultSeconds);
            ResultText.gameObject.SetActive(false);

            if(currentPoint <= 0)
            {
                ResultText.gameObject.SetActive(true);
                ResultText.text = "Game Over...";
                break;
            }
            if(currentPoint >= goalPoint)
            {
                ResultText.gameObject.SetActive(true);
                ResultText.text = "Game Clear!!";
                break;
            }
        }
    }

    void PlayerDealCard()
    {
        var cardObj = Object.Instantiate(CardPrefab,Player.transform);
        var card = DealCard();
        cardObj.SetCard(card.Number,card.Mark,false);
    }

    bool CheckPlayerCard()
    {
        var sumNumber = 0;
        foreach(var card in Player.transform.GetComponentsInChildren<Card>())
        {
            sumNumber += card.UseNumber;
        }
        return (sumNumber < 21);
    }

    bool StandAction()
    {
        var sumPlayerNumber = 0;
        foreach (var card in Player.transform.GetComponentsInChildren<Card>())
        {
            sumPlayerNumber += card.UseNumber;
        }

        var sumDealerNumber = 0;
        foreach (var card in Dealer.transform.GetComponentsInChildren<Card>())
        {
            sumDealerNumber += card.UseNumber;
            if(card.IsReverse)
            {
                card.SetCard(card.Number,card.CurrentMark,false);
            }
        }
        if(!CheckPlayerCard()) return false;
        return sumPlayerNumber > sumDealerNumber;
    }
}
