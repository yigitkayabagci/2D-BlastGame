using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private string finishedText = "Finished";
    
    
    private void Start()
    {
        button.onClick.AddListener(OnLevelButtonClicked);
        RefreshLevelDisplay();
    }
    
    private void OnEnable()
    {
        RefreshLevelDisplay();
    }

    private void OnLevelButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartLevel();
        else
            SceneManager.LoadScene("LevelScene");
    }

    public void RefreshLevelDisplay()
    {
        if (GameManager.Instance != null)
        {
            int curr_level = GameManager.Instance.GetCurrentLevel();
            bool allLevelsCompleted = GameManager.Instance.AreAllLevelsCompleted();
            UpdateLevelDisplay(curr_level, allLevelsCompleted);
        }
        else
        {
            UpdateLevelDisplay(1, false);
        }
    }

    public void UpdateLevelDisplay(int level, bool allLevelsCompleted)
    {
        if (allLevelsCompleted)
        {
            levelText.text = finishedText;
            button.interactable = false; 
        }
        else
        {
            levelText.text = $"Level {level}";
            button.interactable = true;
        }
    }


    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnLevelButtonClicked);
    }
}