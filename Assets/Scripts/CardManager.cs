using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardManager : MonoBehaviour

{
    public static CardManager instance;

    [Header("Card Settings")]
    public int pairs = 6;
    public GameObject cardPrefab;
    public Transform spacer;
    public List<Sprite> spriteList = new List<Sprite>();

    [Header("Scoring")]
    public int matchScore = 100;
    public int mismatchPenalty = 20;
    private int currentScore = 0;

    [Header("UI Elements")]
    public Text scoreText;

    private List<Card> allCards = new List<Card>();
    private List<Card> chosenCards = new List<Card>();
    private bool isComparing = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        LoadScore();
        UpdateScoreUI();
        FillPlayField();
    }

    void FillPlayField()
    {
        List<Sprite> selectedSprites = new List<Sprite>();

        for (int i = 0; i < pairs; i++)
        {
            selectedSprites.Add(spriteList[i]);
            selectedSprites.Add(spriteList[i]);
        }

        // Shuffle
        for (int i = 0; i < selectedSprites.Count; i++)
        {
            Sprite temp = selectedSprites[i];
            int randomIndex = Random.Range(i, selectedSprites.Count);
            selectedSprites[i] = selectedSprites[randomIndex];
            selectedSprites[randomIndex] = temp;
        }

        for (int i = 0; i < selectedSprites.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, spacer);
            Card card = newCard.GetComponent<Card>();
            card.id = i / 2 + 1;
            card.cardFront = selectedSprites[i];
            card.cardBack = cardPrefab.GetComponent<Image>().sprite;
            allCards.Add(card);
        }
    }

    public void AddChosenCard(Card card)
    {
        if (chosenCards.Contains(card)) return;
        chosenCards.Add(card);
    }

    public bool ReadyToCompare()
    {
        return chosenCards.Count == 2;
    }

    public bool IsComparing()
    {
        return isComparing;
    }

    public void StartCompareProcess()
    {
        if (!isComparing)
            StartCoroutine(CompareCards());
    }

    private IEnumerator CompareCards()
    {
        isComparing = true;

        // Wait until both cards have finished flipping
        while (chosenCards[0].IsFlipping || chosenCards[1].IsFlipping)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // extra wait time if needed

        if (chosenCards[0].cardFront == chosenCards[1].cardFront)
        {
            Debug.Log("Match!");
            currentScore += matchScore;
            foreach (var card in chosenCards)
            {
                card.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            Debug.Log("Mismatch!");
            currentScore -= mismatchPenalty;
            foreach (var card in chosenCards)
            {
                card.ForceCloseCard();
            }
        }

        UpdateScoreUI();
        SaveScore();

        chosenCards.Clear();
        isComparing = false;

        if (CheckWinCondition())
        {
            Debug.Log("YOU WON! ??");
            // Optionally you can show a win screen or restart
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString() ;
        }
    }

    void SaveScore()
    {
        PlayerPrefs.SetInt("CardGame_Score", currentScore);
    }

    void LoadScore()
    {
        currentScore = PlayerPrefs.GetInt("CardGame_Score", 0);
    }

    bool CheckWinCondition()
    {
        foreach (var card in allCards)
        {
            if (card.GetComponent<Button>().interactable)
                return false;
        }
        return true;
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteKey("CardGame_Score");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}