using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int totalLevels = 10;

    private bool isGameOver = false;

    private void Awake()
    
    {
        //PersistenceManager.SaveLevel(1);
        
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        currentLevel = PersistenceManager.LoadLevel();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateLevelUI();
    }

    private void OnEnable()
    {
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from scene loading events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void UpdateLevelUI()
    {
        LevelButton[] levelButtons = FindObjectsOfType<LevelButton>();
        foreach (LevelButton button in levelButtons)
        {
            button.RefreshLevelDisplay();
        }
    }

    public int GetCurrentLevel() => currentLevel;

    public bool AreAllLevelsCompleted() => currentLevel > totalLevels;

    public void StartLevel()
    {
        SceneController.Instance.LoadLevelScene();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        SceneController.Instance.LoadMainScene();
    }

    public void CompleteLevel()
    {
        currentLevel++;
        PersistenceManager.SaveLevel(currentLevel);
        ReturnToMainMenu();
    }

}