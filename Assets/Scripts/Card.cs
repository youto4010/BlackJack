using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour,IPointerClickHandler
// OnPointerClick のコールバックを受け取りたいときにインターフェースを実装する。
{
    public enum Mark{
        Heart,
        Diamond,
        Spade,
        Crub,
    }
    // 列挙型,基本的には月や曜日など、特定の値しかとらないデータを表現する際に使用します。

    public bool IsReverse = false;
    
    [Range(1,13)]
    public int Number = 1;
    // 定義域を設定１～13
    public Mark CurrentMark = Mark.Heart;

    public void SetCard(int number,Mark mark,bool isReverse)
    {
        Number = Mathf.Clamp(number,1,13);
        // 与えられた最小 float 値と最大 float 値の範囲に値を制限します。
        CurrentMark = mark;
        IsReverse = isReverse;

        var image = GetComponent<Image>();
        // imageコンポーネントを取得
        if(IsReverse)
        {
            image.color = Color.black;
        }
        else
        {
            image.color = Color.white;
        }
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(!IsReverse);
        }
        // コレクションのすべての要素を1つ1つ取得するときに使用する

        var markObj = transform.Find("Mark");
        // Findメソッドは要素を先頭から検索して、初めに見つかった一つの要素を返します。
        var markText = markObj.GetComponent<Text>();
        // 指定された変数値を元に一致するcase値の制御文を実行します。
        switch (CurrentMark)
        {
            case Mark.Heart:
            markText.text = "♥";
            markText.color = Color.red;
            break;
            case Mark.Diamond:
            markText.text = "♦";
            markText.color = Color.red;
            break;
            case Mark.Spade:
            markText.text = "♠";
            markText.color = Color.black;
            break;
            case Mark.Crub:
            markText.text = "♣";
            markText.color = Color.black;
            break;
        }

        var numberObj = transform.Find("NumberText");
        var numberText = numberObj.GetComponent<Text>();
        if(Number == 1)
        {
            numberText.text = "A";
        }
        else if(Number == 11)
        {
            numberText.text = "J";
        }
        else if(Number == 12)
        {
            numberText.text = "Q";
        }
        else if(Number == 13)
        {
            numberText.text = "K";
        }
        else
        {
            numberText.text = Number.ToString();
            // 変換時に文字列をカスタマイズできます。
        }

        var optionalNumberObj = transform.Find("OptionalNumberText");
        optionalNumberObj.gameObject.SetActive(!IsReverse && Number == 1);
        if(Number == 1)
        {
            var OptionalNumberText = optionalNumberObj.GetComponent<Text>();
            OptionalNumberText.text = UseNumber.ToString();
        }
    }

    private void OnValidate()
    {
        SetCard(Number,CurrentMark,IsReverse);
    }

    public class Data
    {
        public Mark Mark;
        public int Number;
    }

    public bool IsLarge = false;
    public int UseNumber
    {
        get
        {
            if(Number > 10) return 10;
            if(Number == 1)
            {
                return IsLarge ? 11:1;
            }
            return Number;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Number == 1)
        {
            IsLarge = !IsLarge;
            SetCard(Number, CurrentMark, IsReverse);
        }
    }
}
