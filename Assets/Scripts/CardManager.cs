using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour

{
    public List<Sprite> spriteList = new List<Sprite>();
    [SerializeField] private List<GameObject> buttonList = new List<GameObject>();
    [SerializeField] private List<GameObject> hiddenButtonList = new List<GameObject>();
    [Header("How many pairs you want to play")]
    public int pairs;
    [Header("Card prefab button")]
    public GameObject cardPrefab;
    [Header("Parent Spacer to sort cards in")]
    public Transform spacer;
    [Header("Basic Score per match")]
    public int matchScore = 100;

    public static CardManager instance;

    public List<GameObject> choosenCards = new List<GameObject>();
    private bool choosen;
    private int lastMatchId;
    public int choice1;
    public int choice2;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        FillPlayField();
    }

    void FillPlayField()
    {
        for (int i = 0; i < (pairs * 2); i++)
        {
            GameObject newCard = Instantiate(cardPrefab, spacer);
            buttonList.Add(newCard);
            hiddenButtonList.Add(newCard);
        }
        ShuffleCards();
    }

    public void AddChoosenCard(GameObject card)
    {
        choosenCards.Add(card);
    }

    public IEnumerator CompareCards()
    {
        if (choice2 == 0 || choosen)
            yield break;

        choosen = true;
        yield return new WaitForSeconds(0.5f); // small wait before comparing

        if ((choice1 != 0 && choice2 != 0) && (choice1 == choice2))
        {
            lastMatchId = choice1;
            // Correct match, do nothing (keep opened)
            choosenCards.Clear();
        }
        else
        {
            // Wrong match, flip both back
            yield return new WaitForSeconds(0.5f);
            FlipAllBack();
        }

        choice1 = 0;
        choice2 = 0;
        choosen = false;
    }

    void FlipAllBack()
    {
        foreach (GameObject card in choosenCards)
        {
            card.GetComponent<Card>().CloseCard();
        }
        choosenCards.Clear();
    }

    void ShuffleCards()
    {
        int num = 0;
        int cardPairs = buttonList.Count / 2;
        List<GameObject> tempList = new List<GameObject>(buttonList);

        for (int i = 0; i < cardPairs; i++)
        {
            num++;
            for (int j = 0; j < 2; j++)
            {
                int cardIndex = Random.Range(0, tempList.Count);
                Card tempCard = tempList[cardIndex].GetComponent<Card>();
                tempCard.id = num;
                tempCard.cardFront = spriteList[num - 1];
                tempList.RemoveAt(cardIndex);
            }
        }
    }

    void Update() { }
}
