using TMPro;
using UnityEngine;

public class CoffeeShopSequenceManager : MonoBehaviour
{
    public static CoffeeShopSequenceManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Minigame")]
    [SerializeField] private CoffeeCatchMinigame coffeeMinigame;

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
            objectiveText.text =
                "Objective: Enter the coffee shop";
        }
    }

    public void TryEnterCoffeeShop()
    {
        if (!entranceUnlocked)
        {
            Debug.LogWarning(
                "Coffee shop is still locked."
            );

            return;
        }

        if (minigameStarted)
        {
            Debug.LogWarning(
                "Coffee minigame already started."
            );

            return;
        }

        if (minigameCompleted)
        {
            Debug.LogWarning(
                "Coffee minigame already completed."
            );

            return;
        }

        if (coffeeMinigame == null)
        {
            Debug.LogError(
                "CoffeeCatchMinigame is not assigned."
            );

            return;
        }

        minigameStarted = true;

        if (coffeeShopHighlight != null)
        {
            coffeeShopHighlight.DisableHighlight();
        }

        if (objectiveText != null)
        {
            objectiveText.text =
                "Catch the ingredients and avoid the bad items!";
        }

        Debug.Log("Starting coffee catching minigame.");

        coffeeMinigame.StartMinigame();
    }

    public void CompleteCoffeeMinigame()
    {
        if (minigameCompleted)
            return;

        minigameCompleted = true;
        minigameStarted = false;

        if (coffeeShopHighlight != null)
        {
            coffeeShopHighlight.DisableHighlight();
        }

        if (objectiveText != null)
        {
            objectiveText.text =
                "Objective: Score a goal in the park.";
        }

        if (SoccerMinigameManager.Instance != null)
        {
            Debug.Log(
                "Coffee completed. Starting soccer minigame."
            );

            SoccerMinigameManager.Instance
                .StartSoccerMinigame();
        }
        else
        {
            Debug.LogError(
                "SoccerMinigameManager.Instance is null."
            );
        }

        Debug.Log("Coffee minigame completed.");
    }

    public void CancelCoffeeMinigame()
    {
        if (!minigameStarted || minigameCompleted)
            return;

        minigameStarted = false;

        if (objectiveText != null)
        {
            objectiveText.text =
                "Objective: Enter the coffee shop";
        }

        Debug.Log("Coffee minigame cancelled.");
    }
}