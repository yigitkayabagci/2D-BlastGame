using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Game UI")]
    [SerializeField] private GameObject gamePanel;          
    [SerializeField] private TextMeshProUGUI movesText;   

    [Header("Goal UI")]
    [SerializeField] private TextMeshProUGUI boxCountText;
    [SerializeField] private TextMeshProUGUI stoneCountText;
    [SerializeField] private TextMeshProUGUI vaseCountText;
    [SerializeField] private Image boxGoalImage;
    [SerializeField] private Image stoneGoalImage;
    [SerializeField] private Image vaseGoalImage;
    [SerializeField] private GameObject boxTickMark;
    [SerializeField] private GameObject stoneTickMark;
    [SerializeField] private GameObject vaseTickMark;

    [Header("Popup UI")]
    [SerializeField] private PopupManager popupManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowGameUI(int moves)
    {
        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
            UpdateMoves(moves);
        }
        else
        {
        }
    }

    public void UpdateMoves(int moves)
    {
        if (movesText == null)
        {
            movesText = GameObject.Find("MovesText")?.GetComponent<TextMeshProUGUI>();
        }
        if(movesText != null)
            movesText.text = moves.ToString();
    }

    public void UpdateGoals(int boxCount, int stoneCount, int vaseCount)
    {
        UpdateSingleGoal(boxCountText, boxTickMark, boxGoalImage, boxCount);
        UpdateSingleGoal(stoneCountText, stoneTickMark, stoneGoalImage, stoneCount);
        UpdateSingleGoal(vaseCountText, vaseTickMark, vaseGoalImage, vaseCount);
    }
    
    private void UpdateSingleGoal(TextMeshProUGUI countText, GameObject tickMark, Image goalImage, int count)
    {
        if(countText == null)
            return;
        if (count > 0)
        {
            countText.gameObject.SetActive(true);
            tickMark.SetActive(false);
            countText.text = count.ToString();
            goalImage.gameObject.SetActive(true);
        }
        else if (count == 0)
        {
            countText.gameObject.SetActive(false);
            tickMark.SetActive(true);
            goalImage.gameObject.SetActive(true);
        }
    }

    
    //win condition
    public void ShowLevelComplete()
    {
        popupManager.ShowLevelWinPopup(() => 
        {
            GameManager.Instance.CompleteLevel();
            SceneController.Instance.LoadMainScene();
        });
    }

    // Lose condition
    public void ShowLevelFailed()
    {
        popupManager.ShowLevelFailedPopup(
            onRetry: () => { GameManager.Instance.RestartLevel(); },
            onExit: () => { SceneController.Instance.LoadMainScene(); }
        );
    }
}
