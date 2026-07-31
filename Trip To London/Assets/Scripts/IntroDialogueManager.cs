using System.Collections;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroDialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("Name displayed above the dialogue")]
        public string speakerName = "Player";

        [TextArea(2, 5)]
        [Tooltip("Dialogue shown for this line")]
        public string dialogueText;
    }

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button continueButton;

    [Header("Dialogue Lines")]
    [SerializeField] private DialogueLine[] dialogueLines;

    [Header("Player")]
    [SerializeField] private ThirdPersonController playerController;
    [SerializeField] private StarterAssetsInputs playerInputs;

    [Header("Typing Effect")]
    [SerializeField] private bool useTypingEffect = true;
    [SerializeField] private float typingSpeed = 0.025f;

    private int currentLineIndex;
    private bool dialogueActive;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private Coroutine cursorCoroutine;

    public bool DialogueActive => dialogueActive;

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(AdvanceDialogue);
        }

        StartDialogue();
    }

    private void Update()
    {
        if (!dialogueActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            AdvanceDialogue();
        }
    }

    public void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        dialogueActive = true;
        currentLineIndex = 0;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        SetPlayerControl(false);
        UnlockCursor();

        DisplayCurrentLine();

        if (cursorCoroutine != null)
        {
            StopCoroutine(cursorCoroutine);
        }

        cursorCoroutine = StartCoroutine(
            KeepCursorUnlockedAtStart()
        );
    }

    public void AdvanceDialogue()
    {
        if (!dialogueActive)
        {
            return;
        }

        if (isTyping)
        {
            FinishTypingCurrentLine();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (dialogueLines == null ||
            currentLineIndex < 0 ||
            currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine =
            dialogueLines[currentLineIndex];

        if (speakerText != null)
        {
            speakerText.text = currentLine.speakerName;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        string sentence = currentLine.dialogueText;

        if (string.IsNullOrEmpty(sentence))
        {
            sentence = string.Empty;
        }

        if (useTypingEffect)
        {
            typingCoroutine = StartCoroutine(
                TypeDialogue(sentence)
            );
        }
        else
        {
            if (dialogueText != null)
            {
                dialogueText.text = sentence;
            }

            isTyping = false;
        }
    }

    private IEnumerator TypeDialogue(string sentence)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }

        foreach (char letter in sentence)
        {
            if (dialogueText != null)
            {
                dialogueText.text += letter;
            }

            yield return new WaitForSecondsRealtime(
                typingSpeed
            );
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void FinishTypingCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text =
                dialogueLines[currentLineIndex].dialogueText;
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueActive = false;
        isTyping = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (cursorCoroutine != null)
        {
            StopCoroutine(cursorCoroutine);
            cursorCoroutine = null;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        SetPlayerControl(true);
        LockCursor();
    }

    private void SetPlayerControl(bool controlsEnabled)
    {
        if (playerInputs != null)
        {
            playerInputs.move = Vector2.zero;
            playerInputs.look = Vector2.zero;
            playerInputs.sprint = false;
            playerInputs.jump = false;

            playerInputs.enabled = controlsEnabled;
        }

        if (playerController != null)
        {
            playerController.enabled = controlsEnabled;
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator KeepCursorUnlockedAtStart()
    {
        yield return null;

        UnlockCursor();

        yield return new WaitForEndOfFrame();

        UnlockCursor();

        cursorCoroutine = null;
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(
                AdvanceDialogue
            );
        }
    }
}