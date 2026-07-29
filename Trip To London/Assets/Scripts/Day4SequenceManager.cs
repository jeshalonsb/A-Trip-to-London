using System.Collections;
using StarterAssets;
using TMPro;
using UnityEngine;

public class Day4SequenceManager : MonoBehaviour
{
    public static Day4SequenceManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Transform dayFourSpawnPoint;

    [Header("Taxi")]
    [SerializeField] private GameObject taxi;

    [Header("Objective UI")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Day Transition")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float dayTextDuration = 2f;

    [Header("Ending")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TMP_Text endingText;
    [SerializeField] private float endingFadeDuration = 2f;
    [SerializeField]
    private string endingMessage =
        "Thank you for playing A Week in London.";

    private bool dayFourStarted;
    private bool taxiRideActive;
    private bool endingStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
            fadeCanvas.interactable = false;
        }

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }

        if (taxi != null)
        {
            taxi.SetActive(false);
        }
    }

    public void StartDayFour()
    {
        if (dayFourStarted)
            return;

        dayFourStarted = true;

        StartCoroutine(DayFourTransition());
    }

    private IEnumerator DayFourTransition()
    {
        yield return FadeToBlack();

        MovePlayerToSpawn();

        if (dayText != null)
        {
            dayText.text = "DAY 4";
            dayText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(dayTextDuration);

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        if (taxi != null)
        {
            taxi.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Day4SequenceManager: Taxi is not assigned."
            );
        }

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Get into the taxi";
        }

        yield return FadeFromBlack();
    }

    public void TaxiRideStarted()
    {
        if (taxiRideActive)
            return;

        taxiRideActive = true;

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Ride to the bridge";
        }

        Debug.Log("Day 4 taxi ride started.");
    }

    public void TaxiReachedBridge()
    {
        if (endingStarted)
            return;

        endingStarted = true;

        Debug.Log("Taxi reached bridge. Starting ending.");

        StartCoroutine(PlayEnding());
    }

    private IEnumerator PlayEnding()
    {
        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(false);
        }

        yield return FadeToBlack();

        if (endingPanel == null)
        {
            Debug.LogWarning(
                "Day4SequenceManager: Ending Panel is not assigned."
            );

            yield break;
        }

        endingPanel.SetActive(true);

        CanvasGroup endingCanvasGroup =
            endingPanel.GetComponent<CanvasGroup>();

        if (endingText != null)
        {
            endingText.text = endingMessage;
        }

        if (endingCanvasGroup == null)
        {
            endingPanel.AddComponent<CanvasGroup>();
            endingCanvasGroup =
                endingPanel.GetComponent<CanvasGroup>();
        }

        endingCanvasGroup.alpha = 0f;
        endingCanvasGroup.blocksRaycasts = true;
        endingCanvasGroup.interactable = true;

        float timer = 0f;

        while (timer < endingFadeDuration)
        {
            timer += Time.deltaTime;

            endingCanvasGroup.alpha =
                Mathf.Clamp01(
                    timer / endingFadeDuration
                );

            yield return null;
        }

        endingCanvasGroup.alpha = 1f;
    }

    private void MovePlayerToSpawn()
    {
        if (player == null || dayFourSpawnPoint == null)
        {
            Debug.LogWarning(
                "Player or Day Four Spawn Point is missing."
            );

            return;
        }

        CharacterController controller =
            player.GetComponent<CharacterController>();

        ThirdPersonController movement =
            player.GetComponent<ThirdPersonController>();

        player.transform.SetParent(null, true);

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.SetPositionAndRotation(
            dayFourSpawnPoint.position,
            dayFourSpawnPoint.rotation
        );

        Physics.SyncTransforms();

        if (controller != null)
        {
            controller.enabled = true;
        }

        if (movement != null)
        {
            movement.enabled = true;
            movement.SetBusRiding(false);
        }
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeCanvas == null)
            yield break;

        fadeCanvas.blocksRaycasts = true;

        float timer = 0f;
        float startingAlpha = fadeCanvas.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            fadeCanvas.alpha = Mathf.Lerp(
                startingAlpha,
                1f,
                timer / fadeDuration
            );

            yield return null;
        }

        fadeCanvas.alpha = 1f;
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeCanvas == null)
            yield break;

        float timer = 0f;
        float startingAlpha = fadeCanvas.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            fadeCanvas.alpha = Mathf.Lerp(
                startingAlpha,
                0f,
                timer / fadeDuration
            );

            yield return null;
        }

        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;
        fadeCanvas.interactable = false;
    }
}