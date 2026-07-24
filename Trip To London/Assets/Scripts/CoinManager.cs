using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text objectiveText;

    [Header("Hotel")]
    [SerializeField] private BuildingHighlight hotelHighlight;

    private int collectedCoins;
    private int totalCoins;

    public bool AllCoinsCollected { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        totalCoins = FindObjectsByType<CoinPickup>(
            FindObjectsSortMode.None
        ).Length;

        collectedCoins = 0;
        AllCoinsCollected = false;

        if (hotelHighlight != null)
        {
            hotelHighlight.DisableHighlight();
        }

        UpdateUI();
    }

    public void CollectCoin()
    {
        collectedCoins++;

        UpdateUI();

        if (collectedCoins >= totalCoins)
        {
            CompleteCoinObjective();
        }
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text =
                $"Coins: {collectedCoins} / {totalCoins}";
        }
    }

    private void CompleteCoinObjective()
    {
        AllCoinsCollected = true;

        if (objectiveText != null)
        {
            objectiveText.text =
                "Objective: Go check into your hotel (highlighted)";
        }

        if (hotelHighlight != null)
        {
            hotelHighlight.EnableHighlight();
        }
    }
}