using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
    public void LoadMainScene()
    {
        ChangeScene("MainScene");
    }

    public void LoadLevelScene()
    {
        SceneManager.LoadScene("LevelScene");
    }
    
    public async Task LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }
    
    public async void ChangeScene(string sceneName)
    {
        await LoadSceneAsync(sceneName);
    }
}