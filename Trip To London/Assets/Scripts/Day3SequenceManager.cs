using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Day3SequenceManager : MonoBehaviour
{
    public static Day3SequenceManager Instance { get; private set; }

    [Header("Objective")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Selfie Spot")]
    [SerializeField] private GameObject selfieSpotObject;
    [SerializeField] private BuildingHighlight selfieSpotHighlight;

    [Header("Hotel")]
    [SerializeField] private BuildingHighlight hotelHighlight;

    [Header("Selfie Camera")]
    [SerializeField] private Camera selfieCamera;
    [SerializeField] private int screenshotWidth = 1280;
    [SerializeField] private int screenshotHeight = 720;

    [Header("Photo Display")]
    [SerializeField] private GameObject photoPanel;
    [SerializeField] private RawImage photoDisplay;

    [Header("Audio")]
    [SerializeField] private AudioClip pictureClick;

    private bool selfieAvailable;
    private bool selfieTaken;
    private bool takingPhoto;
    private bool playerReturnedToHotel;
    private bool dayFourStarted;

    public bool SelfieTaken => selfieTaken;
    public bool PlayerReturnedToHotel => playerReturnedToHotel;
    public bool CanEnterHotelForDayFour =>
        selfieTaken && playerReturnedToHotel && !dayFourStarted;

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
        if (selfieSpotObject != null)
        {
            selfieSpotObject.SetActive(false);
        }

        if (selfieSpotHighlight != null)
        {
            selfieSpotHighlight.DisableHighlight();
        }

        if (hotelHighlight != null)
        {
            hotelHighlight.DisableHighlight();
        }

        if (selfieCamera != null)
        {
            selfieCamera.enabled = false;
        }

        if (photoPanel != null)
        {
            photoPanel.SetActive(false);
        }
    }

    public void ArriveAtBigBen()
    {
        selfieAvailable = true;

        if (selfieSpotObject != null)
        {
            selfieSpotObject.SetActive(true);
        }

        if (selfieSpotHighlight != null)
        {
            selfieSpotHighlight.EnableHighlight();
        }

        if (objectiveText != null)
        {
            objectiveText.text =
                "Objective: Go to the selfie spot and take a selfie";
        }

        Debug.Log("Player arrived at Big Ben.");
    }

    public void TryTakeSelfie()
    {
        if (!selfieAvailable || selfieTaken || takingPhoto)
            return;

        if (pictureClick != null)
        {
            AudioSource.PlayClipAtPoint(pictureClick, transform.position);
        }

        StartCoroutine(TakeSelfie());
    }

    private IEnumerator TakeSelfie()
    {
        takingPhoto = true;

        if (objectiveText != null)
        {
            objectiveText.text = "Taking selfie...";
        }

        yield return new WaitForEndOfFrame();

        if (selfieCamera == null || photoDisplay == null)
        {
            Debug.LogWarning(
                "Selfie Camera or Photo Display has not been assigned."
            );

            takingPhoto = false;
            yield break;
        }

        RenderTexture screenshotTexture = new RenderTexture(
            screenshotWidth,
            screenshotHeight,
            24
        );

        selfieCamera.targetTexture = screenshotTexture;

        Texture2D screenshot = new Texture2D(
            screenshotWidth,
            screenshotHeight,
            TextureFormat.RGB24,
            false
        );

        selfieCamera.Render();

        RenderTexture previousTexture = RenderTexture.active;
        RenderTexture.active = screenshotTexture;

        screenshot.ReadPixels(
            new Rect(0, 0, screenshotWidth, screenshotHeight),
            0,
            0
        );

        screenshot.Apply();

        RenderTexture.active = previousTexture;
        selfieCamera.targetTexture = null;

        screenshotTexture.Release();
        Destroy(screenshotTexture);

        photoDisplay.texture = screenshot;

        if (photoPanel != null)
        {
            photoPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        selfieTaken = true;
        selfieAvailable = false;
        takingPhoto = false;

        if (selfieSpotHighlight != null)
        {
            selfieSpotHighlight.DisableHighlight();
        }

        // Hotel starts highlighting after the selfie.
        if (hotelHighlight != null)
        {
            hotelHighlight.EnableHighlight();
        }

        if (objectiveText != null)
        {
            objectiveText.text =
                "Objective: Return to the double-decker bus";
        }

        Debug.Log("Selfie taken. Hotel highlight enabled.");
    }

    public void ClosePhoto()
    {
        if (photoPanel != null)
        {
            photoPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called by the bus after dropping the player near the hotel.
    public void ReturnToHotel()
    {
        if (!selfieTaken)
            return;

        playerReturnedToHotel = true;

        if (hotelHighlight != null)
        {
            hotelHighlight.EnableHighlight();
        }

        if (objectiveText != null)
        {
            objectiveText.text =
                "Objective: Walk into the hotel";
        }

        Debug.Log("Player returned to the hotel.");
    }

    public void TryStartDayFour()
    {
        if (!CanEnterHotelForDayFour)
            return;

        dayFourStarted = true;

        if (hotelHighlight != null)
        {
            hotelHighlight.DisableHighlight();
        }

        if (Day4SequenceManager.Instance != null)
        {
            Day4SequenceManager.Instance.StartDayFour();
        }
        else
        {
            Debug.LogWarning(
                "Day4SequenceManager is missing from the scene."
            );
        }
    }
}