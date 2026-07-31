using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Sensitivity")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityValueText;
    [SerializeField] private ThirdPersonController playerController;

    [Header("Audio")]
    [SerializeField] private Slider audioSlider;
    [SerializeField] private TMP_Text audioValueText;

    [Header("Display")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Settings")]
    [SerializeField] private float defaultSensitivity = 1f;
    [SerializeField] private float defaultAudioVolume = 1f;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Dialogue")]
    [SerializeField] private IntroDialogueManager introDialogueManager;

    private bool isPaused;

    private const string SensitivityKey = "LookSensitivity";
    private const string AudioVolumeKey = "AudioVolume";
    private const string FullscreenKey = "Fullscreen";

    public bool IsPaused => isPaused;

    private void Start()
    {
        SetupSliders();
        SetupButtons();
        SetupFullscreenToggle();

        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (introDialogueManager != null &&
            introDialogueManager.DialogueActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void SetupSliders()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.1f;
            sensitivitySlider.maxValue = 5f;
            sensitivitySlider.wholeNumbers = false;

            float savedSensitivity = PlayerPrefs.GetFloat(
                SensitivityKey,
                defaultSensitivity
            );

            sensitivitySlider.SetValueWithoutNotify(
                savedSensitivity
            );

            sensitivitySlider.onValueChanged.AddListener(
                SetSensitivity
            );

            SetSensitivity(savedSensitivity);
        }

        if (audioSlider != null)
        {
            audioSlider.minValue = 0f;
            audioSlider.maxValue = 1f;
            audioSlider.wholeNumbers = false;

            float savedAudioVolume = PlayerPrefs.GetFloat(
                AudioVolumeKey,
                defaultAudioVolume
            );

            audioSlider.SetValueWithoutNotify(
                savedAudioVolume
            );

            audioSlider.onValueChanged.AddListener(
                SetAudioVolume
            );

            SetAudioVolume(savedAudioVolume);
        }
    }

    private void SetupButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(
                ResumeGame
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                ReturnToMainMenu
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(
                QuitGame
            );
        }
    }

    private void SetupFullscreenToggle()
    {
        if (fullscreenToggle == null)
        {
            return;
        }

        bool savedFullscreen =
            PlayerPrefs.GetInt(
                FullscreenKey,
                Screen.fullScreen ? 1 : 0
            ) == 1;

        Screen.fullScreen = savedFullscreen;

        fullscreenToggle.SetIsOnWithoutNotify(
            savedFullscreen
        );

        fullscreenToggle.onValueChanged.AddListener(
            SetFullscreen
        );
    }

    public void PauseGame()
    {
        if (introDialogueManager != null &&
            introDialogueManager.DialogueActive)
        {
            return;
        }

        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        if (introDialogueManager != null &&
            introDialogueManager.DialogueActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    public void SetFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;

        PlayerPrefs.SetInt(
            FullscreenKey,
            fullscreen ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    public void SetSensitivity(float sensitivity)
    {
        if (playerController != null)
        {
            playerController.LookSensitivity = sensitivity;
        }

        if (sensitivityValueText != null)
        {
            sensitivityValueText.text =
                sensitivity.ToString("0.0");
        }

        PlayerPrefs.SetFloat(
            SensitivityKey,
            sensitivity
        );

        PlayerPrefs.Save();
    }

    public void SetAudioVolume(float volume)
    {
        AudioListener.volume = volume;

        if (audioValueText != null)
        {
            audioValueText.text =
                Mathf.RoundToInt(volume * 100f) + "%";
        }

        PlayerPrefs.SetFloat(
            AudioVolumeKey,
            volume
        );

        PlayerPrefs.Save();
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(
                SetSensitivity
            );
        }

        if (audioSlider != null)
        {
            audioSlider.onValueChanged.RemoveListener(
                SetAudioVolume
            );
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(
                SetFullscreen
            );
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(
                ResumeGame
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(
                ReturnToMainMenu
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(
                QuitGame
            );
        }
    }
}