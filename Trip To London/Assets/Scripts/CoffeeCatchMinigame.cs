using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoffeeCatchMinigame : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private RectTransform cup;

    [Header("Falling Item Prefabs")]
    [SerializeField] private FallingCoffeeItem[] itemPrefabs;

    [Header("Game Settings")]
    [SerializeField] private int pointsNeeded = 15;
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float spawnInterval = 0.8f;
    [SerializeField] private float spawnEdgePadding = 20f;

    [Header("Cup Settings")]
    [SerializeField] private float cupEdgePadding = 20f;

    [Header("UI Text")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text resultText;

    [Header("Completion")]
    [SerializeField] private float completionDelay = 2f;
    [SerializeField] private float restartDelay = 2f;

    private int currentPoints;
    private int currentLives;

    private bool gameActive;
    private bool roundRestarting;

    private Coroutine spawningCoroutine;
    private Coroutine resultCoroutine;

    public RectTransform Cup => cup;
    public RectTransform GameArea => gameArea;
    public bool GameActive => gameActive;

    private void Awake()
    {
        if (gamePanel != null)
        {
            gamePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!gameActive)
            return;

        MoveCupWithMouse();
    }

    public void StartMinigame()
    {
        if (gameActive || roundRestarting)
            return;

        if (!ValidateReferences())
            return;

        gamePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        currentPoints = 0;
        currentLives = startingLives;
        gameActive = true;

        ClearItems();
        CenterCup();
        UpdateUI();

        if (instructionText != null)
        {
            instructionText.text =
                "Catch coffee beans, milk, and sugar!\nAvoid trash and salt!";
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        spawningCoroutine = StartCoroutine(SpawnLoop());

        Debug.Log("Coffee catching minigame started.");
    }

    private bool ValidateReferences()
    {
        if (gamePanel == null)
        {
            Debug.LogError("Game Panel is not assigned.");
            return false;
        }

        if (gameArea == null)
        {
            Debug.LogError("Game Area is not assigned.");
            return false;
        }

        if (itemContainer == null)
        {
            Debug.LogError("Item Container is not assigned.");
            return false;
        }

        if (cup == null)
        {
            Debug.LogError("Cup is not assigned.");
            return false;
        }

        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogError("No falling item prefabs are assigned.");
            return false;
        }

        return true;
    }

    private void MoveCupWithMouse()
    {
        if (gameArea == null || cup == null)
            return;

        Vector2 screenMousePosition;

        if (Mouse.current != null)
        {
            screenMousePosition =
                Mouse.current.position.ReadValue();
        }
        else
        {
            screenMousePosition =
                Input.mousePosition;
        }

        Canvas canvas =
            gameArea.GetComponentInParent<Canvas>();

        Camera canvasCamera = null;

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = canvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gameArea,
                screenMousePosition,
                canvasCamera,
                out Vector2 localMousePosition))
        {
            return;
        }

        float halfAreaWidth =
            gameArea.rect.width * 0.5f;

        float halfCupWidth =
            cup.rect.width * 0.5f;

        float minimumX =
            -halfAreaWidth +
            halfCupWidth +
            cupEdgePadding;

        float maximumX =
            halfAreaWidth -
            halfCupWidth -
            cupEdgePadding;

        Vector2 newPosition =
            cup.anchoredPosition;

        newPosition.x = Mathf.Clamp(
            localMousePosition.x,
            minimumX,
            maximumX
        );

        cup.anchoredPosition = newPosition;
    }

    private void CenterCup()
    {
        Vector2 cupPosition = cup.anchoredPosition;
        cupPosition.x = 0f;
        cup.anchoredPosition = cupPosition;
    }

    private IEnumerator SpawnLoop()
    {
        while (gameActive)
        {
            SpawnItem();

            yield return new WaitForSeconds(
                spawnInterval
            );
        }
    }

    private void SpawnItem()
    {
        if (!gameActive)
            return;

        if (itemPrefabs == null || itemPrefabs.Length == 0)
            return;

        int randomIndex = Random.Range(0, itemPrefabs.Length);

        FallingCoffeeItem selectedPrefab =
            itemPrefabs[randomIndex];

        if (selectedPrefab == null)
        {
            Debug.LogWarning(
                "Item prefab slot " +
                randomIndex +
                " is empty."
            );

            return;
        }

        FallingCoffeeItem newItem = Instantiate(
            selectedPrefab,
            itemContainer
        );

        RectTransform itemRect =
            newItem.GetComponent<RectTransform>();

        if (itemRect == null)
        {
            Debug.LogError(
                newItem.name +
                " does not have a RectTransform."
            );

            Destroy(newItem.gameObject);
            return;
        }

        // Reset the prefab's UI transform.
        itemRect.anchorMin = new Vector2(0.5f, 0.5f);
        itemRect.anchorMax = new Vector2(0.5f, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.sizeDelta = new Vector2(70, 70);
        itemRect.localScale = Vector3.one;
        itemRect.localRotation = Quaternion.identity;

        float containerHalfWidth =
            itemContainer.rect.width * 0.5f;

        float itemHalfWidth =
            itemRect.rect.width * 0.5f;

        // Small safety margin from each edge.
        float edgePadding = 10f;

        float minimumX =
            -containerHalfWidth +
            itemHalfWidth +
            edgePadding;

        float maximumX =
            containerHalfWidth -
            itemHalfWidth -
            edgePadding;

        float randomX = Random.Range(
            minimumX,
            maximumX
        );

        float spawnY =
            itemContainer.rect.height * 0.5f +
            itemRect.rect.height * 0.5f;

        itemRect.anchoredPosition =
            new Vector2(randomX, spawnY);

        newItem.Initialize(this);

        Debug.Log(
            "Spawned " +
            newItem.name +
            " at X: " +
            randomX +
            " | Container width: " +
            itemContainer.rect.width
        );
    }

    public void CatchItem(
        FallingCoffeeItem.ItemType itemType)
    {
        if (!gameActive)
            return;

        switch (itemType)
        {
            case FallingCoffeeItem.ItemType.CoffeeBean:
                AddPoints(2);
                ShowResult("+2 Coffee Bean");
                break;

            case FallingCoffeeItem.ItemType.Milk:
                AddPoints(2);
                ShowResult("+2 Milk");
                break;

            case FallingCoffeeItem.ItemType.Sugar:
                AddPoints(1);
                ShowResult("+1 Sugar");
                break;

            case FallingCoffeeItem.ItemType.Trash:
                LoseLives(1);
                ShowResult("Trash! -1 Life");
                break;

            case FallingCoffeeItem.ItemType.Salt:
                LoseLives(2);
                ShowResult("Salt! -2 Lives");
                break;
        }

        UpdateUI();

        if (currentPoints >= pointsNeeded)
        {
            CompleteGame();
        }
        else if (currentLives <= 0)
        {
            StartCoroutine(RestartRound());
        }
    }

    public void MissItem(
        FallingCoffeeItem.ItemType itemType)
    {
        if (!gameActive)
            return;

        bool goodIngredient =
            itemType == FallingCoffeeItem.ItemType.CoffeeBean ||
            itemType == FallingCoffeeItem.ItemType.Milk ||
            itemType == FallingCoffeeItem.ItemType.Sugar;

        if (!goodIngredient)
            return;

        LoseLives(1);
        ShowResult("Missed ingredient! -1 Life");
        UpdateUI();

        if (currentLives <= 0)
        {
            StartCoroutine(RestartRound());
        }
    }

    private void AddPoints(int amount)
    {
        currentPoints += amount;

        currentPoints = Mathf.Clamp(
            currentPoints,
            0,
            pointsNeeded
        );
    }

    private void LoseLives(int amount)
    {
        currentLives -= amount;

        currentLives = Mathf.Max(
            currentLives,
            0
        );
    }

    private void CompleteGame()
    {
        if (!gameActive)
            return;

        gameActive = false;

        StopSpawning();
        ClearItems();

        StartCoroutine(CompleteAfterDelay());
    }

    private IEnumerator CompleteAfterDelay()
    {
        if (resultText != null)
        {
            resultText.text = "Coffee Complete!";
            resultText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(
            completionDelay
        );

        if (CoffeeShopSequenceManager.Instance != null)
        {
            CoffeeShopSequenceManager.Instance
                .CompleteCoffeeMinigame();
        }
        else
        {
            Debug.LogError(
                "CoffeeShopSequenceManager.Instance is null."
            );
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gamePanel.SetActive(false);
    }

    private IEnumerator RestartRound()
    {
        if (roundRestarting)
            yield break;

        roundRestarting = true;
        gameActive = false;

        StopSpawning();
        ClearItems();

        if (resultText != null)
        {
            resultText.text = "Out of lives! Try again.";
            resultText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(
            restartDelay
        );

        currentPoints = 0;
        currentLives = startingLives;

        CenterCup();
        UpdateUI();

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        roundRestarting = false;
        gameActive = true;

        spawningCoroutine = StartCoroutine(
            SpawnLoop()
        );
    }

    private void ShowResult(string message)
    {
        if (resultText == null)
            return;

        if (resultCoroutine != null)
        {
            StopCoroutine(resultCoroutine);
        }

        resultCoroutine = StartCoroutine(
            ShowResultTemporarily(message)
        );
    }

    private IEnumerator ShowResultTemporarily(
        string message)
    {
        resultText.text = message;
        resultText.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.65f);

        if (gameActive)
        {
            resultText.gameObject.SetActive(false);
        }

        resultCoroutine = null;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "Points: " +
                currentPoints +
                " / " +
                pointsNeeded;
        }

        if (livesText != null)
        {
            livesText.text =
                "Lives: " +
                currentLives;
        }
    }

    private void StopSpawning()
    {
        if (spawningCoroutine != null)
        {
            StopCoroutine(spawningCoroutine);
            spawningCoroutine = null;
        }
    }

    private void ClearItems()
    {
        if (itemContainer == null)
            return;

        FallingCoffeeItem[] items =
            itemContainer.GetComponentsInChildren<
                FallingCoffeeItem>(true);

        foreach (FallingCoffeeItem item in items)
        {
            Destroy(item.gameObject);
        }
    }
}