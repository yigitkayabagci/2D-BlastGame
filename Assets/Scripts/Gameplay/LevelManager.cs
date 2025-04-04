using UnityEngine;
using System;
using Models;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int moveCount;
    [SerializeField] private int currentLevel = 1;
    
    [Header("Level Goals")]
    [SerializeField] private int boxCount;
    [SerializeField] private int stoneCount;
    [SerializeField] private int vaseCount;
    
    public GameObject gridPanelBackground;
    
    public float cellSize = 65f;
    public float spacing = 0f;
    
    private LevelData currentLevelData;
    
    public event Action OnLevelCompleted;
   public event Action OnLevelFailed;

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
        UIManager.Instance.ShowGameUI(moveCount);
        LoadLevel(GameManager.Instance.GetCurrentLevel());
    }

    public void LoadLevel(int levelNumber)
    {
        currentLevel = levelNumber;
    
        TextAsset levelJson = Resources.Load<TextAsset>($"Levels/level_{levelNumber:D2}");
        if (levelJson != null)
        {
            currentLevelData = JsonUtility.FromJson<LevelData>(levelJson.text);
            moveCount = currentLevelData.move_count;
    
            UIManager.Instance.UpdateMoves(moveCount);
    
            CountObjectives();
            UpdateObjectivesUI();
            UpdateGridPanelBackgroundSize(currentLevelData);
    
            GridManager.Instance.width = currentLevelData.grid_width;
            GridManager.Instance.height = currentLevelData.grid_height;
            GridManager.Instance.CreateGridFromData(currentLevelData);
        }
        else
        {
            Debug.LogError("level yok");
        }
    }
    
    private void UpdateObjectivesUI()
    {
        UIManager.Instance.UpdateGoals(boxCount, stoneCount, vaseCount);
    }
    
    private void CountObjectives()
    {
        boxCount = 0;
        stoneCount = 0;
        vaseCount = 0;
        
        foreach (string item in currentLevelData.grid)
        {
            if (item == "bo") boxCount++;
            else if (item == "s") stoneCount++;
            else if (item == "v") vaseCount++;
        }
    }

    public void DecreaseMoveCount()
    {
        moveCount--;
        UIManager.Instance.UpdateMoves(moveCount);
    }
    
    public bool AreObjectivesCleared()
    {
        int remainingBox = 0;
        int remainingStone = 0;
        int remainingVase = 0;
        
        GridManager gridManager = GridManager.Instance;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                GridItem item = gridManager.GetItemAt(x, y);
                if (item is Box) remainingBox++;
                else if (item is Stone) remainingStone++;
                else if (item is Vase) remainingVase++;
            }
        }
        return remainingBox == 0 && remainingStone == 0 && remainingVase == 0;
    }
    
    public void CheckLevelObjectives()
    {
        if (AreObjectivesCleared())
        {
            GridManager gridManager = GridManager.Instance;
            gridManager.CleanupInactiveCubes();
            CompleteLevelSuccess();
        }
        else if (moveCount <= 0)
        {
            GridManager gridManager = GridManager.Instance;
            gridManager.CleanupInactiveCubes();
            CompleteLevelFailure();
        }
    }

    public void CompleteLevelSuccess()
    {
        OnLevelCompleted?.Invoke();
        UIManager.Instance.ShowLevelComplete();
    }

    public void CompleteLevelFailure()
    {
        OnLevelFailed?.Invoke();
        UIManager.Instance.ShowLevelFailed();
    }
    
    public void OnObstacleDestroyed(string obstacleType)
    {
        if (obstacleType == "Box")
            boxCount = Mathf.Max(0, boxCount - 1);
        else if (obstacleType == "Stone")
            stoneCount = Mathf.Max(0, stoneCount - 1);
        else if (obstacleType == "Vase")
            vaseCount = Mathf.Max(0, vaseCount - 1);
    
        UpdateObjectivesUI();
        CheckLevelObjectives();
    }
    
    public void UpdateGridPanelBackgroundSize(LevelData levelData)
    {
        RectTransform gridPanelRect = gridPanelBackground.GetComponent<RectTransform>();
        float panelWidth = levelData.grid_width * (cellSize + spacing);
        float panelHeight = levelData.grid_height * (cellSize + spacing);
        gridPanelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
    }
}
