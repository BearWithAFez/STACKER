using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class PlayGames : MonoBehaviour {

	void Start () {
        PlayGamesClientConfiguration conf = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(conf);
        PlayGamesPlatform.Activate();

        SignIn();
    }

    void SignIn()
    {
        Social.localUser.Authenticate(success => { });
    }

    #region Achievements
    public static void UnlockAchievement(string id)
    {
        Social.ReportProgress(id, 100, success => { });
    }

    public static void IncrementAchievement(string id, int stepsToIncrement)
    {
        PlayGamesPlatform.Instance.IncrementAchievement(id, stepsToIncrement, success => { });
    }

    public static void ShowAchievementsUI() {
        Social.ShowAchievementsUI();
    }
    #endregion

    public static void AddScoreToLeaderboard(string leaderboardId, long score)
    {
        Social.ReportScore(score, leaderboardId, success => { });
    }

    public static void ShowLeaderboardsUI()
    {
        Social.ShowLeaderboardUI();
    }
}
