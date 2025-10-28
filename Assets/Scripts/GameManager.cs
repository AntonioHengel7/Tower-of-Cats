using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Flow")]
    [SerializeField] private GameObject spawnerRoot; 

    private static bool AutoStartOnLoad = false;
    private bool isPaused;
    private bool gameStarted;
    private int score;

    void Awake()
    {
        Debug.Log("GM Awake");
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Start on the main menu
        Time.timeScale = 0f; // freeze gameplay until Start is pressed
        gameStarted = false;

        if (startPanel) startPanel.SetActive(true);
        if (pausePanel) pausePanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);

        // Keep the spawner off until Start is pressed
        if (spawnerRoot) spawnerRoot.SetActive(false);

        score = 0;
        UpdateScoreUI();

        if (AutoStartOnLoad)
        {
            AutoStartOnLoad = true;
            StartGame(); // enables spawnerRoot and unpauses
            score = 0;
            UpdateScoreUI();
        }
    }

    void Update()
    {
        // Only allow pause after the game has actually started and we're not on GameOver
        if (gameStarted &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame &&
            (!gameOverPanel || !gameOverPanel.activeSelf))
        {
            TogglePause();
        }
    }

    // -------- Start Button hook --------
    public void StartGame()
    {
        Debug.Log("GM StartGame");
        if (gameStarted) return;
        gameStarted = true;

        if (startPanel) startPanel.SetActive(false);
        if (spawnerRoot) spawnerRoot.SetActive(true);

        Time.timeScale = 1f;
        SetScore(0);
    }

    public void AddScore(int delta)
    {
        score = Mathf.Max(0, score + delta);
        UpdateScoreUI();
    }

    public void SetScore(int value)
    {
        score = Mathf.Max(0, value);
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = $"Score: {score}";
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pausePanel) pausePanel.SetActive(isPaused);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        AutoStartOnLoad = true;         // New line
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        Awake();
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    // -------- Optional UI Button hooks --------
    public void Resume() { if (isPaused) TogglePause(); }
    public void Pause() { if (!isPaused) TogglePause(); }
    public void RestartButton() { Restart(); }
    public void QuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}



