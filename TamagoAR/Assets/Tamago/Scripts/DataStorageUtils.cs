using UnityEngine;

public static class DataStorageUtils {

    public static readonly string PREFS_STAR_COUNTER = PACKAGE_PREFIX + "STAR_COUNTER";
    public static readonly string PREFS_CARROT_COUNTER = PACKAGE_PREFIX + "CARROT_COUNTER";
    private static readonly string PACKAGE_PREFIX = "com.kakaboc.tamagoar.";

    public static void SaveGameState(GameState state) {
        PlayerPrefs.SetInt(PREFS_STAR_COUNTER, state.starsBalance);
        PlayerPrefs.Save();
    }

    public static GameState GetSavedGameState() {
        GameState state = new GameState
        {
            starsBalance = PlayerPrefs.GetInt(PREFS_STAR_COUNTER, 0),
            carrotBalance = PlayerPrefs.GetInt(PREFS_CARROT_COUNTER, 0)
        };
        return state;
    }

    public static void SaveStarCounter(int starCounter) {
        PlayerPrefs.SetInt(PREFS_STAR_COUNTER, starCounter);
        PlayerPrefs.Save();
    }

    public static void SaveCarrotCounter(int carrotCounter)
    {
        PlayerPrefs.SetInt(PREFS_CARROT_COUNTER, carrotCounter);
        PlayerPrefs.Save();
    }

    public static int GetSavedStarCounter() {
        return PlayerPrefs.GetInt(PREFS_STAR_COUNTER, 0);
    }

    public static int GetSavedCarrotCounter()
    {
        return PlayerPrefs.GetInt(PREFS_CARROT_COUNTER, 0);
    }
}
