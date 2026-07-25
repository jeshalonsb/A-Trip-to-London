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

    [Header("Building Highlights")]
    [SerializeField] private BuildingHighlight hotelHighlight;
    [SerializeField] private BuildingHighlight coffeeShopHighlight;

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
        if (dayTransitionCanvas != null)
        {
            dayTransitionCanvas.alpha = 0f;
            dayTransitionCanvas.interactable = false;
            dayTransitionCanvas.blocksRaycasts = false;
        }

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        // The coffee shop should not be highlighted during Day 1.
        if (coffeeShopHighlight != null)
        {
            coffeeShopHighlight.DisableHighlight();
        }
    }

    public void UnlockHotelEntrance()
    {
        hotelEntranceUnlocked = true;

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Enter the hotel";
        }
    }

    public void TryEnterHotel()
    {
        if (!hotelEntranceUnlocked || transitionStarted)
            return;

        transitionStarted = true;
        StartCoroutine(StartDayTwoTransition());
    }

    private IEnumerator StartDayTwoTransition()
    {
        if (dayTransitionCanvas != null)
        {
            dayTransitionCanvas.interactable = true;
            dayTransitionCanvas.blocksRaycasts = true;
        }

        yield return Fade(0f, 1f);

        // The screen is now fully black.
        // Switch the building highlights here so the player cannot see them change.
        if (hotelHighlight != null)
        {
            hotelHighlight.DisableHighlight();
        }

        if (coffeeShopHighlight != null)
        {
            coffeeShopHighlight.EnableHighlight();
        }

        if (dayText != null)
        {
            dayText.text = "DAY 2";
            dayText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(dayTextDuration);

        MovePlayerToDayTwoSpawn();

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Go to the coffee shop";
        }

        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        yield return Fade(1f, 0f);

        if (dayTransitionCanvas != null)
        {
            dayTransitionCanvas.interactable = false;
            dayTransitionCanvas.blocksRaycasts = false;
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (dayTransitionCanvas == null)
            yield break;

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

    private void MovePlayerToDayTwoSpawn()
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