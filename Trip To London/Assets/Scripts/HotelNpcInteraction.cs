using TMPro;
using UnityEngine;

public class HotelNpcInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text interactionText;

    [Header("Player Control")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Dialogue")]
    [TextArea(3, 6)]
    [SerializeField]
    private string receptionistDialogue =
        "Receptionist: Welcome! Your room is ready. You can head inside now.";

    [TextArea(3, 6)]
    [SerializeField]
    private string incompleteCoinsDialogue =
        "Receptionist: Please explore the city first, then come back to check in.";

    private bool playerNearby;
    private bool dialogueCompleted;
    private bool allowedToCheckIn;
    private bool dialogueOpen;
    private bool movementWasEnabled;

    private void Start()
    {
        Time.timeScale = 1f;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerNearby || dialogueCompleted || dialogueOpen)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ShowDialogue();
        }
    }

    private void ShowDialogue()
    {
        allowedToCheckIn =
            CoinManager.Instance == null ||
            CoinManager.Instance.AllCoinsCollected;

        dialogueOpen = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(true);

            dialogueText.text = allowedToCheckIn
                ? receptionistDialogue
                : incompleteCoinsDialogue;
        }
        else
        {
            Debug.LogWarning("Dialogue Text is not assigned on HotelNpcInteraction.");
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        if (playerMovementScript != null)
        {
            movementWasEnabled = playerMovementScript.enabled;
            playerMovementScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseDialogue()
    {
        dialogueOpen = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (playerMovementScript != null && movementWasEnabled)
        {
            playerMovementScript.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!allowedToCheckIn)
        {
            if (playerNearby && interactionText != null)
            {
                interactionText.text = "Press E to speak";
                interactionText.gameObject.SetActive(true);
            }

            return;
        }

        dialogueCompleted = true;

        if (HotelSequenceManager.Instance != null)
        {
            HotelSequenceManager.Instance.UnlockHotelEntrance();
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerNearby = true;

        if (!dialogueCompleted && !dialogueOpen && interactionText != null)
        {
            interactionText.text = "Press E to interact";
            interactionText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerNearby = true;

        if (!dialogueCompleted && !dialogueOpen && interactionText != null)
        {
            interactionText.text = "Press E to interact";
            interactionText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerNearby = false;

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player") ||
               other.transform.root.CompareTag("Player");
    }
}