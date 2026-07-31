using System.Collections;
using UnityEngine;
using TMPro;

public class SoccerMinigameManager : MonoBehaviour
{
    public static SoccerMinigameManager Instance { get; private set; }

    [Header("Soccer Objects")]
    [SerializeField] private GameObject soccerMinigameObject;
    [SerializeField] private SoccerBallKick soccerBall;
    [SerializeField] private Transform ballSpawnPoint;

    [Header("UI")]
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text resultText;

    [Header("Timing")]
    [SerializeField] private float goalMessageDuration = 2f;
    [SerializeField] private float ballResetDelay = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip goalSound;

    private bool minigameActive;
    private bool objectiveCompleted;
    private bool resettingBall;

    private Rigidbody ballRigidbody;
    private Coroutine goalMessageCoroutine;

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
        if (soccerBall != null)
        {
            ballRigidbody = soccerBall.GetComponent<Rigidbody>();
            soccerBall.SetMinigameActive(false);
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        if (soccerMinigameObject != null)
        {
            soccerMinigameObject.SetActive(false);
        }
    }

    public void StartSoccerMinigame()
    {
        if (minigameActive)
            return;

        minigameActive = true;
        objectiveCompleted = false;
        resettingBall = false;

        if (soccerMinigameObject != null)
        {
            soccerMinigameObject.SetActive(true);
        }

        ResetBall();

        if (soccerBall != null)
        {
            soccerBall.SetMinigameActive(true);
        }

        if (objectiveText != null)
        {
            objectiveText.text = "Objective: Score a goal in the park";
        }

        Debug.Log("Soccer minigame started.");
    }

    public void ScoreGoal()
    {
        if (!minigameActive || resettingBall)
            return;

        resettingBall = true;

        Debug.Log("Goal scored!");

        if (!objectiveCompleted)
        {
            objectiveCompleted = true;

            if (HotelSequenceManager.Instance != null)
            {
                HotelSequenceManager.Instance.UnlockHotelForDayThree();
            }

            Debug.Log("Soccer objective completed.");
        }

        if (goalMessageCoroutine != null)
        {
            StopCoroutine(goalMessageCoroutine);
        }

        goalMessageCoroutine = StartCoroutine(GoalSequence());
        StartCoroutine(ResetBallSequence());
    }

    private IEnumerator GoalSequence()
    {
        if (resultText != null)
        {
            resultText.text = "GOAL!";
            resultText.gameObject.SetActive(true);
        }

        if (goalSound != null)
        {
            AudioSource.PlayClipAtPoint(goalSound, transform.position);
        }

        yield return new WaitForSeconds(goalMessageDuration);

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        goalMessageCoroutine = null;
    }

    private IEnumerator ResetBallSequence()
    {
        yield return new WaitForSeconds(ballResetDelay);

        ResetBall();

        resettingBall = false;
    }

    private void ResetBall()
    {
        if (soccerBall == null || ballSpawnPoint == null)
            return;

        if (ballRigidbody == null)
        {
            ballRigidbody = soccerBall.GetComponent<Rigidbody>();
        }

        if (ballRigidbody != null)
        {
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }

        soccerBall.transform.position = ballSpawnPoint.position;
        soccerBall.transform.rotation = ballSpawnPoint.rotation;
    }
}