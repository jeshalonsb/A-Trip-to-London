using System.Collections;
using TMPro;
using UnityEngine;

public class HotelSequenceManager : MonoBehaviour
{
    public static HotelSequenceManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private CanvasGroup dayTransitionCanvas;
    [SerializeField] private TMP_Text dayText;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform dayTwoSpawnPoint;

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float dayTextDuration = 2f;

    private bool hotelEntranceUnlocked;
    private bool transitionStarted;

    public bool HotelEntranceUnlocked => hotelEntranceUnlocked;

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
        dayTransitionCanvas.alpha = 0f;
        dayTransitionCanvas.interactable = false;
        dayTransitionCanvas.blocksRaycasts = false;

        dayText.gameObject.SetActive(false);
    }

    public void UnlockHotelEntrance()
    {
        hotelEntranceUnlocked = true;

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Enter the hotel";
        }

        Debug.Log("Hotel entrance unlocked.");
    }

    public void TryEnterHotel()
    {
        if (!hotelEntranceUnlocked)
            return;

        if (transitionStarted)
            return;

        transitionStarted = true;
        StartCoroutine(StartDayTwoTransition());
    }

    private IEnumerator StartDayTwoTransition()
    {
        dayTransitionCanvas.blocksRaycasts = true;

        yield return Fade(0f, 1f);

        dayText.text = "DAY 2";
        dayText.gameObject.SetActive(true);

        yield return new WaitForSeconds(dayTextDuration);

        MovePlayer();

        if (objectiveText != null)
        {
            objectiveText.text =
                "Objective: Go to the café and speak to someone";
        }

        dayText.gameObject.SetActive(false);

        yield return Fade(1f, 0f);

        dayTransitionCanvas.blocksRaycasts = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            dayTransitionCanvas.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                timer / fadeDuration
            );

            yield return null;
        }

        dayTransitionCanvas.alpha = endAlpha;
    }

    private void MovePlayer()
    {
        if (player == null || dayTwoSpawnPoint == null)
            return;

        CharacterController controller =
            player.GetComponent<CharacterController>();

        if (controller != null)
        {
            controller.enabled = false;
        }

        player.position = dayTwoSpawnPoint.position;
        player.rotation = dayTwoSpawnPoint.rotation;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}