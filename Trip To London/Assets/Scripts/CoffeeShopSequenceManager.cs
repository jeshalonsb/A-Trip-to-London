using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoffeeShopSequenceManager : MonoBehaviour
{
    public static CoffeeShopSequenceManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Minigame")]
    [SerializeField] private CoffeeMinigame coffeeMinigame;

    [Header("Highlights")]
    [SerializeField] private BuildingHighlight coffeeShopHighlight;

    private bool entranceUnlocked;
    private bool minigameStarted;
    private bool minigameCompleted;

    public bool EntranceUnlocked => entranceUnlocked;
    public bool MinigameCompleted => minigameCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void UnlockCoffeeShop()
    {
        if (entranceUnlocked)
            return;

        entranceUnlocked = true;

        Debug.Log("Coffee shop entrance unlocked.");

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Enter the coffee shop";
        }

    }
    public void TryEnterCoffeeShop()
    {
        if (!entranceUnlocked)
        {
            Debug.LogWarning("Coffee shop is still locked.");
            return;
        }

        if (minigameStarted)
        {
            Debug.LogWarning("Minigame already started.");
            return;
        }

        if (minigameCompleted)
        {
            Debug.LogWarning("Minigame already completed.");
            return;
        }

        if (coffeeMinigame == null)
        {
            Debug.LogError("CoffeeMinigame is NULL.");
            return;
        }

        minigameStarted = true;

        Debug.Log("Starting Coffee Minigame!");

        coffeeMinigame.BeginMinigame();
    }
    public void CompleteCoffeeMinigame()
    {
        minigameCompleted = true;
        minigameStarted = false;

        if (coffeeShopHighlight != null)
        {
            coffeeShopHighlight.DisableHighlight();
        }

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Score a goal in the park.";
        }

        if (SoccerMinigameManager.Instance != null)
        {
            Debug.Log("Soccer manager found. Starting soccer minigame.");
            SoccerMinigameManager.Instance.StartSoccerMinigame();
        }
        else
        {
            Debug.LogError("SoccerMinigameManager.Instance is NULL.");
        }

        Debug.Log("Coffee minigame completed");
    }
}
