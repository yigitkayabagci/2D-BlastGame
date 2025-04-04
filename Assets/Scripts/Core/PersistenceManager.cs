using UnityEngine;

public static class PersistenceManager
{
    private const string LevelKey = "LastPlayedLevel";

    public static void SaveLevel(int level)
    {
        PlayerPrefs.SetInt(LevelKey, level);
        PlayerPrefs.Save();
    }

    public static int LoadLevel()
    {
        return PlayerPrefs.GetInt(LevelKey, 1);
    }
}