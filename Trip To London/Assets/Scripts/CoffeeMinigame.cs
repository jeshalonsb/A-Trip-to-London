using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;

public class CoffeeMinigame : MonoBehaviour
{
    [Header("Minigame UI")]
    [SerializeField] private GameObject minigameCanvas;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text statusText;

    [Header("Player")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    private int currentStep;
    private bool gameActive;
    private bool movementWasEnabled;

    private readonly string[] coffeeSteps =
    {
        "Coffee",
        "Milk",
        "Sugar",
        "Serve"
    };
    private void Start()
    {
        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(false);
        }
    }
    public void BeginMinigame()
    {
        currentStep = 0;
        gameActive = true;

        if (minigameCanvas != null)
        {
            minigameCanvas.SetActive(true);
        }

        if (playerMovementScript != null )
        {
            movementWasEnabled = playerMovementScript.enabled;
            playerMovementScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (instructionText  != null)
        {
            instructionText.text = "Make a coffee in the correct order: \nCoffee, Milk, Sugar, then Enjoy";
        }

        UpdateStatus();
    }
    public void AddCoffee()
    {
        CheckStep("Coffee");
    }
    public void AddMilk()
    {
        CheckStep("Milk");
    }
    public void AddSugar()
    {
        CheckStep("Sugar");
    }
    public void ServeCoffee()
    {
        CheckStep("Serve");
    }
    private void CheckStep(string selectedStep)
    {
        if (!gameActive)
            return;

        if (selectedStep == coffeeSteps[currentStep])
        {
            currentStep++;

            if (currentStep >= coffeeSteps.Length)
            {
                FinishMinigame();
                return;
            }

            UpdateStatus();
        }
        else
        {
            currentStep = 0;

            if (statusText != null)
            {
                statusText.text = "Wrong order! Start again with Coffee";
            }
        }
    }
    private void UpdateStatus()
    {
        if (statusText == null)
            return;

        switch (currentStep)
        {
            case 0: statusText.text = "Choose the coffee.";
                break;

            case 1: statusText.text = "Coffee added. Now add milk.";
                break;

            case 2: statusText.text = "Milk added. Now add sugar";
                break;

            case 3: statusText.text = "Sugar added. Enjoy your coffee!";
                break;
        }
    }
    private void FinishMinigame()
    {
        gameActive = false;

        if (statusText != null)
        {
            statusText.text = "Coffee Complete!";
        }

        Invoke(nameof(CloseMinigame), 1.25f);
    }
    private void CloseMinigame()
    {
        if ( minigameCanvas != null )
        {
            minigameCanvas.SetActive(false);
        }

        if (playerMovementScript != null && movementWasEnabled)
        {
            playerMovementScript.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (CoffeeShopSequenceManager.Instance != null)
        {
            CoffeeShopSequenceManager.Instance.CompleteCoffeeMinigame();
        }
    }
}
