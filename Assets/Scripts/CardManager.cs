using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    [Header("Game Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject gridSelectionPanel;
    public GameObject gameplayUIPanel;
    public GameObject cardArea;

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public Transform spacer;
    public List<Sprite> spriteList = new List<Sprite>();
    public int rows = 3;
    public int columns = 4;
    public int matchScore = 100;
    public int mismatchPenalty = 20;
    public float startTime = 120f;

    [Header("UI Elements")]
    public Text scoreText;
    public Text comboCountText;
    public Text timerText;
    public Text clicksText;

    private int selectedRows, selectedColumns;
    private int currentScore, correctComboCount, totalClicks;
    private float timer;
    private bool timerRunning, isComparing;

    private readonly List<Card> allCards = new();
    private readonly List<Card> chosenCards = new();

    private void Awake()
    {
        instance ??= this;
    }

    private void Start()
    {
        timer = startTime;
        timerRunning = true;  // ❌ This starts the timer when the scene loads (main menu!)

        // ...
        FillPlayField();
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

    private void ResetStats()
    {
        currentScore = correctComboCount = totalClicks = 0;
        timer = startTime;
        timerRunning = true;
    }

    private void UpdateUI()
    {
        UpdateScoreUI();
        UpdateComboCountUI();
        UpdateClicksUI();
        UpdateTimerUI();
    }

    private void UpdateScoreUI() => scoreText.text = $"Score: {currentScore}";
    private void UpdateComboCountUI() => comboCountText.text = $"Combo: {correctComboCount}";
    private void UpdateClicksUI() => clicksText.text = $"Clicks: {totalClicks}";
    private void UpdateTimerUI() => timerText.text = $"Time: {Mathf.Round(timer)}";

    public void IncrementClicks()
    {
        totalClicks++;
        UpdateClicksUI();
    }

    private void FillPlayField()
    {
        int totalCards = rows * columns;
        if (totalCards % 2 != 0 || totalCards / 2 > spriteList.Count)
        {
            Debug.LogError("Invalid card configuration.");
            return;
        }

        List<Sprite> selectedSprites = GetShuffledSprites(totalCards / 2);
        foreach (var sprite in selectedSprites)
        {
            GameObject cardObj = Instantiate(cardPrefab, spacer);
            Card card = cardObj.GetComponent<Card>();
            card.Initialize(sprite);
            allCards.Add(card);
        }

        ArrangeCardsInGrid();
    }

    private List<Sprite> GetShuffledSprites(int pairCount)
    {
        List<Sprite> pool = new(spriteList);
        Shuffle(pool);
        List<Sprite> pairs = new();
        for (int i = 0; i < pairCount; i++)
        {
            pairs.Add(pool[i]);
            pairs.Add(pool[i]);
        }
        Shuffle(pairs);
        return pairs;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private void ArrangeCardsInGrid()
    {
        var grid = spacer.GetComponent<GridLayoutGroup>() ?? spacer.gameObject.AddComponent<GridLayoutGroup>();
        RectTransform rect = spacer.GetComponent<RectTransform>();
        grid.cellSize = new Vector2(rect.rect.width / columns, rect.rect.height / rows);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
    }

    public void AddChosenCard(Card card)
    {
        if (chosenCards.Contains(card)) return;
        chosenCards.Add(card);
        if (chosenCards.Count == 2)
            StartCoroutine(CompareCards());
    }

    private IEnumerator CompareCards()
    {
        isComparing = true;

        yield return new WaitUntil(() => !chosenCards[0].IsFlipping && !chosenCards[1].IsFlipping);
        yield return new WaitForSeconds(0.5f);

        if (chosenCards[0].IsMatch(chosenCards[1]))
        {
            correctComboCount++;
            currentScore += matchScore;
            foreach (var card in chosenCards)
                card.DisableInteraction();

            SoundManager.instance.PlaySound(SoundManager.instance.matchSound);
        }
        else
        {
            currentScore -= mismatchPenalty;
            foreach (var card in chosenCards)
                card.ForceCloseCard();

            SoundManager.instance.PlaySound(SoundManager.instance.mismatchSound);
        }

        SaveScore();
        UpdateScoreUI();
        UpdateComboCountUI();

        chosenCards.Clear();
        isComparing = false;

        if (CheckWinCondition())
            ShowPanel(winPanel);
    }

    private void SaveScore() => PlayerPrefs.SetInt("CardGame_Score", currentScore);

    private bool CheckWinCondition()
    {
        foreach (var card in allCards)
            if (card.IsInteractable()) return false;

        timerRunning = false;
        return true;
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel != null) panel.SetActive(true);
    }

    private void EndGame()
    {
        timerRunning = false;
        ShowPanel(losePanel);
    }

    public void PlayAgain() => ResetGame();
    public void TryAgain() => ResetGame();

    public void ResetGame()
    {
        timerRunning = false;
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);
        ResetStats();
        ClearAllCards();
        FillPlayField();
        UpdateUI();
    }

    public void StopGameAndGoToMainMenu()
    {
        timerRunning = false;
        gameplayUIPanel?.SetActive(false);
        cardArea?.SetActive(false);
        gridSelectionPanel?.SetActive(true);
        ClearAllCards();
        ResetStats();
        UpdateUI();
    }

    public void StartGame2x2() => SetGridAndStart(2, 2);
    public void StartGame2x3() => SetGridAndStart(2, 3);

    public void OnPlayButtonPressed()
    {
        if (selectedRows > 0 && selectedColumns > 0)
            StartGame(selectedRows, selectedColumns);
        else
            Debug.LogWarning("Please select a grid size first!");
    }

    private void SetGridAndStart(int r, int c)
    {
        selectedRows = r;
        selectedColumns = c;
    }

    public void StartGame(int r, int c)
    {
        rows = r;
        columns = c;
        gridSelectionPanel?.SetActive(false);
        gameplayUIPanel?.SetActive(true);
        cardArea?.SetActive(true);
        ResetGame();
    }

    private void ClearAllCards()
    {
        foreach (var card in allCards)
            Destroy(card.gameObject);
        allCards.Clear();
        chosenCards.Clear();
    }

    public bool IsComparing() => isComparing;
}
