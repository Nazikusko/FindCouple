using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardObjectController : MonoBehaviour, IPointerClickHandler
{
    private Sprite cardBackIcon;
    private Sprite cardFrontIcon;


    private int currenScorePoints;

    public int CurrenScorePoints
    {
        get
        {
            return currenScorePoints;
        }
    }

    private bool isDeleted = false;
    public bool IsDeleted
    {
        get
        {
            return isDeleted;
        }
    }

    private Image image;

    private void Awake()
    {
        image = gameObject.AddComponent<Image>();
    }

    public void SetIcons(Sprite cardbackIcon, Sprite cardfronticon)
    {
        image.sprite = cardFrontIcon = cardfronticon;
        cardBackIcon = cardbackIcon;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (GameController.Use.CurrentGameStatus == GameStatus.TwoCardsOpen ||
            GameController.Use.CurrentGameStatus == GameStatus.TimerCountDown
            || isDeleted)
            return;

        image.sprite = cardFrontIcon;
        GameController.Use.CardClick(gameObject);
    }

    public void TurnCardBack()
    {
        image.sprite = cardBackIcon;
    }

    public void DeleteCard()
    {
        image.color = new Color(1f, 1f, 1f, 0f);
        isDeleted = true;
    }

    public void ResetScore()
    {
        currenScorePoints = 0;
    }
    public void SetScore(int score)
    {
        currenScorePoints = score;
    }
}
