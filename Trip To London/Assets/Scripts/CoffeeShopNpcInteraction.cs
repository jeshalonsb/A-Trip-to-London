using TMPro;
using UnityEngine;

public class CoffeeShopNpcInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text interactionText;

    [Header("Player")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Dialogue")]
    [TextArea(3, 6)]
    [SerializeField]
    private string npcDialogue =
        "Barista: Welcome! Head inside and you can try making your own coffee.";

    private bool playerNearby;
    private bool dialogueOpen;
    private bool dialogueCompleted;
    private bool movementWasEnabled;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerNearby || dialogueOpen || dialogueCompleted)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenDialogue();
        }
    }

    private void OpenDialogue()
    {
        dialogueOpen = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.text = npcDialogue;
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
        dialogueCompleted = true;

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

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        if (CoffeeShopSequenceManager.Instance != null)
        {
            CoffeeShopSequenceManager.Instance.UnlockCoffeeShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        playerNearby = true;

        if (!dialogueCompleted && interactionText != null)
        {
            interactionText.text = "Press E to Interact";
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
            interactionText.text = "Press E to Interact";
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

