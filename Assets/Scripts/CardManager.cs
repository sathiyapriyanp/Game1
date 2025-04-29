using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;
    [Header("End Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

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
    [Header("Panels and UI Objects")]
    public GameObject gridSelectionPanel; // Main Menu
    public GameObject gameplayUIPanel;    // Gameplay UI (timer, score etc.)
    public GameObject cardArea;           // Where Cards are placed
   // public GameObject saveButton;

   private int selectedRows;
private int selectedColumns;





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

        // Step 1: Shuffle the sprite list before selecting pairs
        List<Sprite> shuffledSpriteList = new List<Sprite>(spriteList);
        for (int i = 0; i < shuffledSpriteList.Count; i++)
        {
            Sprite temp = shuffledSpriteList[i];
            int randomIndex = Random.Range(i, shuffledSpriteList.Count);
            shuffledSpriteList[i] = shuffledSpriteList[randomIndex];
            shuffledSpriteList[randomIndex] = temp;
        }

        // Step 2: Select and duplicate the needed sprites
        List<Sprite> selectedSprites = new List<Sprite>();
        for (int i = 0; i < neededPairs; i++)
        {
            selectedSprites.Add(shuffledSpriteList[i]);
            selectedSprites.Add(shuffledSpriteList[i]);
        }

        // Step 3: Shuffle the card positions
        for (int i = 0; i < selectedSprites.Count; i++)
        {
            Sprite temp = selectedSprites[i];
            int randomIndex = Random.Range(i, selectedSprites.Count);
            selectedSprites[i] = selectedSprites[randomIndex];
            selectedSprites[randomIndex] = temp;
        }

        // Step 4: Create card objects
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
        UpdateScoreUI();        
        UpdateComboCountUI();   

        chosenCards.Clear();
        isComparing = false;

        if (CheckWinCondition())
        {
            Debug.Log("YOU WON");
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
                return false; // Still cards left
        }

        // 🎯 All cards matched!
        timerRunning = false; // Stop timer
        if (winPanel != null)
            winPanel.SetActive(true); // Show Win Screen
        return true;
    }
    public void PlayAgain()
    {
        ResetGame();
    }

    void EndGame()
    {
        Debug.Log("Game Over! Your score: " + currentScore);

        timerRunning = false;

        if (losePanel != null)
            losePanel.SetActive(true); // ➡️ Show Lose Panel
    }
    public void TryAgain()
    {
        ResetGame();
    }

    public void ResetGame()
    {
        timerRunning = false;

        // Hide any end panels
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Reset score and state
        currentScore = 0;
        totalClicks = 0;
        correctComboCount = 0;
        timer = startTime;

        // Clear and refill board with same grid
        ClearAllCards();
        FillPlayField();

        // Reset UI
        UpdateScoreUI();
        UpdateComboCountUI();
        UpdateClicksUI();
        UpdateTimerUI();

    }
    public void StartGame2x2()
    {
        selectedRows = 2;
        selectedColumns = 2;
    }

    public void StartGame2x3()
    {
        selectedRows = 2;
        selectedColumns = 3;
    }
    public void OnPlayButtonPressed()
    {
        if (selectedRows > 0 && selectedColumns > 0)
        {
            StartGame(selectedRows, selectedColumns);
        }
        else
        {
            Debug.LogWarning("Please select a grid size first!");
        }
    }
    public void StartGame(int selectedRows, int selectedColumns)
    {
        rows = selectedRows;
        columns = selectedColumns;

        gridSelectionPanel.SetActive(false);   // Hide Main Menu
        gameplayUIPanel.SetActive(true);        // Show Gameplay UI
        cardArea.SetActive(true);               // Show cards area

       /* if (saveButton != null)
            saveButton.SetActive(true);*/

        totalClicks = 0;
        correctComboCount = 0;
        currentScore = 0;
        timer = startTime;
        timerRunning = true;

        ClearAllCards();
        FillPlayField();
        UpdateScoreUI();
        UpdateComboCountUI();
        UpdateClicksUI();
    }
    void ClearAllCards()
    {
        foreach (var card in allCards)
        {
            Destroy(card.gameObject);
        }
        allCards.Clear();
        chosenCards.Clear();
    }
    public void StopGameAndGoToMainMenu()
    {
        // Stop timer
        timerRunning = false;

        // Hide gameplay UI and card area
        if (gameplayUIPanel != null)
            gameplayUIPanel.SetActive(false);

        if (cardArea != null)
            cardArea.SetActive(false);

        // Show main menu
        if (gridSelectionPanel != null)
            gridSelectionPanel.SetActive(true);

        // Clear cards from previous game
        ClearAllCards();

        // Reset game state
        currentScore = 0;
        totalClicks = 0;
        correctComboCount = 0;
        UpdateScoreUI();
        UpdateComboCountUI();
        UpdateClicksUI();
    }


}
