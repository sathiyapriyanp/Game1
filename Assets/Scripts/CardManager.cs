using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    [Header("Card Settings")]
    public int pairs = 6; // The number of pairs of cards
    public GameObject cardPrefab;
    public Transform spacer;
    public List<Sprite> spriteList = new List<Sprite>();

    [Header("Scoring")]
    public int matchScore = 100;
    public int mismatchPenalty = 20;
    private int currentScore = 0;

    [Header("UI Elements")]
    public Text scoreText;
    public Text comboCountText;
    public Text timerText;
    public Text clicksText; 

    private List<Card> allCards = new List<Card>();
    private List<Card> chosenCards = new List<Card>();
    private bool isComparing = false;

    // Timer variables
    public float startTime = 120f; 
    private float timer;
    public bool timerRunning = false;

    // Combo count
    private int correctComboCount = 0;

    // Click counter
    private int totalClicks = 0;

    [Header("Layout Settings")]
    public int rows = 3; 
    public int columns = 4; 



    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        timer = startTime;
        timerRunning = true;

        currentScore = 0;
        correctComboCount = 0;
        totalClicks = 0;

        UpdateScoreUI();
        UpdateComboCountUI();
        UpdateClicksUI();
        UpdateTimerUI();

        FillPlayField();

        comboCountText.gameObject.SetActive(true);
        clicksText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime; 
            if (timer <= 0)
            {
                timer = 0;
                EndGame(); 
            }
            UpdateTimerUI();
        }
    }

    void ArrangeCardsInGrid()
    {
        GridLayoutGroup grid = spacer.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = spacer.gameObject.AddComponent<GridLayoutGroup>();
        }

        
        RectTransform spacerRect = spacer.GetComponent<RectTransform>();
        float cellWidth = spacerRect.rect.width / columns;
        float cellHeight = spacerRect.rect.height / rows;

        grid.cellSize = new Vector2(cellWidth, cellHeight);
        grid.spacing = new Vector2(5, 5); 
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
    }

    void FillPlayField()
    {
        int totalCards = rows * columns;

        if (totalCards % 2 != 0)
        {
            Debug.LogError("Total number of cards must be even!");
            return;
        }

        int neededPairs = totalCards / 2;

        if (neededPairs > spriteList.Count)
        {
            Debug.LogError("Not enough sprites for the requested number of pairs!");
            return;
        }

        List<Sprite> selectedSprites = new List<Sprite>();

       
        for (int i = 0; i < neededPairs; i++)
        {
            selectedSprites.Add(spriteList[i]);
            selectedSprites.Add(spriteList[i]);
        }

       
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

        ArrangeCardsInGrid();
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

        // Wait until flipping is done
        while (chosenCards[0].IsFlipping || chosenCards[1].IsFlipping)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // Check if matched
        if (chosenCards[0].cardFront == chosenCards[1].cardFront)
        {
            correctComboCount++;
            currentScore += matchScore;

            foreach (var card in chosenCards)
            {
                card.GetComponent<Button>().interactable = false;
            }

            SoundManager.instance.PlaySound(SoundManager.instance.matchSound);
        }
        else
        {
            
            currentScore -= mismatchPenalty;

            foreach (var card in chosenCards)
            {
                card.ForceCloseCard();
            }

            SoundManager.instance.PlaySound(SoundManager.instance.mismatchSound);
        }

        SaveScore();
        UpdateScoreUI();        // ❗ update immediately after changing score
        UpdateComboCountUI();   // ❗ update combo immediately

        chosenCards.Clear();
        isComparing = false;

        if (CheckWinCondition())
        {
            Debug.Log("YOU WON! 🎉");
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    void UpdateComboCountUI()
    {
        if (comboCountText != null)
        {
            comboCountText.text = "Combo: " + correctComboCount.ToString();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Round(timer).ToString();
        }
    }

    public void UpdateClicksUI() 
    {
        if (clicksText != null)
        {
            clicksText.text = "Clicks: " + totalClicks.ToString();
        }
    }
    public void IncrementClicks()
    {
        totalClicks++;
        UpdateClicksUI();
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

    void EndGame()
    {
       
        Debug.Log("Game Over! Your score: " + currentScore);
    }

    public void ResetGame()
    {
        PlayerPrefs.DeleteKey("CardGame_Score");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);


    }

}
