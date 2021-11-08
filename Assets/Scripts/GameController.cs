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


    private GameStatus currentGameStatus;
    public GameStatus CurrentGameStatus
    {
        get { return currentGameStatus; }
    }

    private List<CardItem> ListCardsPrefab; //all scriptable cards in recources folder
    private List<CardItem> ListCardsOnTable; //current list scriptable objects on game table
    private List<CardObjectController> ListCardsObjOnTable; //current list of cards scripts controller
    private GameObject[] openedCards = new GameObject[2];
    private int numberOfCardsInDB = 0;
    private int currentScore = 0;

    private void Awake()
    {
        currentGameStatus = GameStatus.TimerCountDown;
        ListCardsPrefab = LoadScriptableObjectsFromRecources();
    }

    private void Start()
    {
        ScoreText.text = "SCORE: " + currentScore.ToString();
        MessageBox.HideMessageBox();

        if (currentGameStatus == GameStatus.LoadError)
            return;

        ListCardsOnTable = CreateCardsListForGame(ListCardsPrefab);

        //Clear CardsContainer default icons
        foreach (Transform item in CardsContainer.transform)
        {
            Destroy(item.gameObject);
        }

        ListCardsObjOnTable = PlaceCardsToUI(ListCardsOnTable);

        StartCoroutine(StartTimerForShowingAllCards());
    }

    private List<CardItem> CreateCardsListForGame(List<CardItem> list)
    {
        CardItem[] masCardsPrefab = list.ToArray();
        List<CardItem> listCardsOnTable = new List<CardItem>();

        // select 8 random cards from the scriptable database (if there are more than 8)
        for (int i = 0; i < CARDSTYPEONTABLE; i++)
        {
            int randNum = Random.Range(0, numberOfCardsInDB);

            while (masCardsPrefab[randNum] == null)
            {
                randNum++;
                if (randNum >= numberOfCardsInDB) randNum = 0;
            }

            listCardsOnTable.Add(masCardsPrefab[randNum]);//added 2 cards
            listCardsOnTable.Add(masCardsPrefab[randNum]);

            masCardsPrefab[randNum] = null;
        }

        //shuffle the cards on the table
        for (int i = 0; i < CARDSONTABLE * 3; i++)
        {
            int randIndex1 = Random.Range(0, listCardsOnTable.Count);
            int randIndex2 = Random.Range(0, listCardsOnTable.Count);

            CardItem card = listCardsOnTable[randIndex1];
            listCardsOnTable[randIndex1] = listCardsOnTable[randIndex2];
            listCardsOnTable[randIndex2] = card;
        }
        return listCardsOnTable;
    }

    private List<CardItem> LoadScriptableObjectsFromRecources()
    {
        try
        {
            List<CardItem> listCardsPrefab = new List<CardItem>();

            UnityEngine.Object[] cardsNames = Resources.LoadAll("ScriptableObjects/CardsData", typeof(CardItem));
            numberOfCardsInDB = cardsNames.Length;

            if (numberOfCardsInDB < CARDSTYPEONTABLE)
                throw new Exception("Not enough cards in the database");

            //Load recources in scriptable objects
            for (int i = 0; i < numberOfCardsInDB; i++)
            {
                listCardsPrefab.Add(Resources.Load<CardItem>("ScriptableObjects/CardsData/" + cardsNames[i].name));
            }
            return listCardsPrefab;

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            MessageBox.ShowMessage(ex.Message);
            currentGameStatus = GameStatus.LoadError;
            return null;
        }
    }

    private List<CardObjectController> PlaceCardsToUI(List<CardItem> listCardsOnTable)
    {
        List<CardObjectController> listCardsObjOnTable = new List<CardObjectController>();
        foreach (CardItem card in listCardsOnTable)
        {
            var uiIcon = new GameObject(card.IconName);
            uiIcon.transform.parent = CardsContainer.transform;
            CardObjectController cObj = uiIcon.AddComponent<CardObjectController>();
            cObj.SetIcons(ÑardBackIcon, card.Icon);
            cObj.SetScore(card.ScorePoints);
            listCardsObjOnTable.Add(cObj);
        }
        return listCardsObjOnTable;
    }

    private IEnumerator StartTimerForShowingAllCards()
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
