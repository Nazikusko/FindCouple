using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum GameStatus
{
    Idle,
    TimerCountDown,
    OneCardOpen,
    TwoCardsOpen,
    Win,
    LoadError
}

public class GameController : SingleTone<GameController>
{
    const int CARDSTYPEONTABLE = 8;
    const int CARDSONTABLE = 16;
    const int SHOWCARDSTIME = 10;


    public GameObject CardsContainer;
    public Sprite ÑardBackIcon;
    public Text TimerText;
    public Text ScoreText;
    public MessageBoxController MessageBox;


    GameStatus currentGameStatus;
    public GameStatus CurrentGameStatus
    {
        get { return currentGameStatus; }
    }

    private List<CardItem> ListCardsPrefab = new List<CardItem>();
    private List<CardItem> ListCardsOnTable = new List<CardItem>();
    private List<CardObjectController> ListCardsObjOnTable = new List<CardObjectController>();
    private GameObject[] openedCards = new GameObject[2];
    private int numberOfCardsInDB = 0;
    private int currentScore = 0;

    private void Awake()
    {
        currentGameStatus = GameStatus.TimerCountDown;
        string[] cardsObjPaths;
        try
        {
            cardsObjPaths = Directory.GetFiles("Assets/Resources/ScriptableObjects/CardsData", "*.asset");
            numberOfCardsInDB = cardsObjPaths.Length;

            if (numberOfCardsInDB < CARDSTYPEONTABLE)
                throw new Exception("Not enough cards in the database");

            //Load recources in scriptable objects
            for (int i = 0; i < numberOfCardsInDB; i++)
            {
                cardsObjPaths[i] = Path.GetFileNameWithoutExtension(cardsObjPaths[i]);
                ListCardsPrefab.Add(Resources.Load<CardItem>("ScriptableObjects/CardsData/" + cardsObjPaths[i]));
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            currentGameStatus = GameStatus.LoadError;
        }
    }

    private void Start()
    {
        if (currentGameStatus == GameStatus.LoadError)
            return;

        CardItem[] masCardsPrefab = ListCardsPrefab.ToArray();
        ScoreText.text = "SCORE: " + currentScore.ToString();
        MessageBox.HideMessageBox();

        // select 8 random cards from the database (if there are more than 8)
        for (int i = 0; i < CARDSTYPEONTABLE; i++)
        {
            int randNum = Random.Range(0, numberOfCardsInDB);

            while (masCardsPrefab[randNum] == null)
            {
                randNum++;
                if (randNum >= numberOfCardsInDB) randNum = 0;
            }

            ListCardsOnTable.Add(masCardsPrefab[randNum]);//added 2 cards
            ListCardsOnTable.Add(masCardsPrefab[randNum]);

            masCardsPrefab[randNum] = null;
        }


        //shuffle the cards on the table
        for (int i = 0; i < CARDSONTABLE * 2; i++)
        {
            int randIndex1 = Random.Range(0, ListCardsOnTable.Count);
            int randIndex2 = Random.Range(0, ListCardsOnTable.Count);

            CardItem card = ListCardsOnTable[randIndex1];
            ListCardsOnTable[randIndex1] = ListCardsOnTable[randIndex2];
            ListCardsOnTable[randIndex2] = card;
        }

        //Clear CardsContainer default icons
        foreach (Transform item in CardsContainer.transform)
        {
            Destroy(item.gameObject);
        }

        PlaceCardsToUI();

        StartCoroutine(StartTimerShowingAllCards());
    }

    private void PlaceCardsToUI()
    {
        foreach (CardItem card in ListCardsOnTable)
        {
            var uiIcon = new GameObject(card.IconName);
            uiIcon.transform.parent = CardsContainer.transform;
            CardObjectController cObj = uiIcon.AddComponent<CardObjectController>();
            cObj.SetIcons(ÑardBackIcon, card.Icon);
            cObj.SetScore(card.ScorePoints);
            ListCardsObjOnTable.Add(cObj);
        }
    }

    private IEnumerator StartTimerShowingAllCards()
    {
        int timer = SHOWCARDSTIME;
        TimerText.gameObject.SetActive(true);

        do
        {
            TimerText.text = timer.ToString();
            yield return new WaitForSecondsRealtime(1f);
            timer--;
            if (timer == 0)
            {
                foreach (var item in ListCardsObjOnTable)
                {
                    item.TurnCardBack();
                }
            }
        } while (timer > 0);
        TimerText.gameObject.SetActive(false);
        currentGameStatus = GameStatus.Idle;
    }

    public void CardClick(GameObject go)
    {
        if (currentGameStatus == GameStatus.Idle)
        {
            currentGameStatus = GameStatus.OneCardOpen;
            openedCards[0] = go;
            return;
        }
        if (currentGameStatus == GameStatus.OneCardOpen)
        {
            currentGameStatus = GameStatus.TwoCardsOpen;
            openedCards[1] = go;
            StartCoroutine(OpenedTwoCards());

        }
    }

    IEnumerator OpenedTwoCards()
    {
        yield return new WaitForSecondsRealtime(3f);
        CheckOpenedCards();
        openedCards[0] = null;
        openedCards[1] = null;
        currentGameStatus = GameStatus.Idle;
    }

    private void CheckOpenedCards()
    {
        CardObjectController cObj = new CardObjectController();

        if (openedCards[0].name == openedCards[1].name)
        {
            foreach (var item in openedCards)
            {
                cObj = item.GetComponent<CardObjectController>();
                cObj.DeleteCard();
                currentScore += cObj.CurrenScorePoints;
                ScoreText.text = "SCORE: " + currentScore.ToString();

                if (CheckEndOfGame())
                {
                    MessageBox.ShowMessage("YOU WIN!\nYOUR SCORE: " + currentScore.ToString());
                    currentGameStatus = GameStatus.Win;
                }
            }
        }
        else
        {
            foreach (var item in openedCards)
            {
                cObj = item.GetComponent<CardObjectController>();
                cObj.TurnCardBack();
                cObj.ResetScore();
            }
        }
    }
    private bool CheckEndOfGame()
    {
        foreach (var item in ListCardsObjOnTable)
        {
            if (!item.IsDeleted) return false;
        }
        return true;
    }

    public void StartNewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Exit()
    {
        Application.Quit();
    }
}
