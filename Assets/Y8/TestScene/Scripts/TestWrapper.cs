using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Y8API;

public class TestWrapper : MonoBehaviour
{
    public class SaveFileData
    {
        public string stringValue;
        public bool boolValue;
        public float floatValue;
    }

    [SerializeField]
    private TextMeshProUGUI debugText;

    // ── Subscribe to the two game-lifecycle ad events ─────────────────────────

    private void OnEnable()
    {
        Y8.Instance.OnAdPauseGame += HandleAdPause;
        Y8.Instance.OnAdResumeGame += HandleAdResume;
        Y8.Instance.OnAuthError += HandleAuthError;
    }

    private void OnDisable()
    {
        Y8.Instance.OnAdPauseGame -= HandleAdPause;
        Y8.Instance.OnAdResumeGame -= HandleAdResume;
        Y8.Instance.OnAuthError -= HandleAuthError;
    }

    private void HandleAdPause()
    {
        LogDebug("[TestWrapper] Ad started — pause game (e.g. Time.timeScale = 0)");
    }

    private void HandleAdResume()
    {
        LogDebug("[TestWrapper] Ad finished — resume game (e.g. Time.timeScale = 1)");
    }

    private void HandleAuthError(AuthError err)
    {
        LogDebug($"[TestWrapper] Auth error [{err.code}]: {err.message}");
    }

    // ── Auth ──────────────────────────────────────────────────────────────────
    public async void ButtonAutoLoginAsync()
    {
        JsResponse<Y8User> response = await Y8.Instance.AutoLoginAsync();
        LogResponse(response);
    }

    public async void ButtonLoginAsync()
    {
        JsResponse<Y8User> response = await Y8.Instance.LoginAsync();
        LogResponse(response);
    }

    public async void ButtonLogoutAsync()
    {
        await Y8.Instance.LogoutAsync();
        LogDebug($"[TestWrapper] session cleared");
    }

    // ── Ads ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Interstitial ad (game loaded, before play)
    /// Pause/resume handled automatically via OnAdPauseGame / OnAdResumeGame.
    /// </summary>
    public async void ButtonShowAdStartAsync() => await ShowAdAsync(AdType.start);

    /// <summary>
    /// Interstitial ad (player paused)
    /// Pause/resume handled automatically via OnAdPauseGame / OnAdResumeGame.
    /// </summary>
    public async void ButtonShowAdPauseAsync() => await ShowAdAsync(AdType.pause);

    /// <summary>
    /// Interstitial ad (between levels).
    /// Pause/resume handled automatically via OnAdPauseGame / OnAdResumeGame.
    /// </summary>
    public async void ButtonShowAdNextAsync() => await ShowAdAsync(AdType.next);

    /// <summary>
    /// Interstitial ad (menu / options).
    /// Pause/resume handled automatically via OnAdPauseGame / OnAdResumeGame.
    /// </summary>
    public async void ButtonShowAdBrowseAsync() => await ShowAdAsync(AdType.browse);

    private async Task ShowAdAsync(AdType adType)
    {
        JsResponse<AdBreakInfo> result = await Y8.Instance.ShowAdAsync(adType);
        LogDebug($"[TestWrapper] ShowAd done — status: {result.Data?.Status}");
    }

    /// <summary>
    /// Rewarded ad. Check AdBreakInfo.Status for the outcome.
    /// AdBreakStatus.Viewed = grant reward.
    /// </summary>
    public async void ButtonShowRewardedAd()
    {
        JsResponse<AdBreakInfo> result = await Y8.Instance.ShowAdAsync(
            AdType.reward,
            "test-reward"
        );

        if (!result.IsSuccess)
        {
            LogDebug("[TestWrapper] Rewarded ad skipped (no GameId)");
            return;
        }

        switch (result.Data.Status)
        {
            case AdBreakStatus.Viewed:
                LogDebug("[TestWrapper] Rewarded ad fully watched → grant reward");
                break;
            case AdBreakStatus.Dismissed:
                LogDebug("[TestWrapper] Rewarded ad dismissed → no reward");
                break;
            case AdBreakStatus.NoFill:
                LogDebug("[TestWrapper] No ad available right now → try later");
                break;
            case AdBreakStatus.Error:
                LogDebug("[TestWrapper] Ad error → handle gracefully");
                break;
        }
    }

    // ── Achievements ──────────────────────────────────────────────────────────

    public async void ButtonShowAchievementsAsync()
    {
        await Y8.Instance.ShowAchievementsAsync();
        LogDebug("[TestWrapper] Achievements closed");
    }

    public async void ButtonGetAchievementsAsync()
    {
        JsResponse<AchievementsData> response = await Y8.Instance.GetAchievements();
        LogResponse(response);
    }

    public async void ButtonAwardAchievementAsync()
    {
        JsResponse<AchievementsData> achievementsData = await Y8.Instance.GetAchievements();
        Achievement[] achievements = achievementsData.Data.achievements;
        if (achievements == null || achievements.Length == 0)
        {
            LogDebug("No achievements available");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, achievements.Length);
        Achievement randomAchievement = achievements[randomIndex];
        JsResponse<AchievementSave> response = await Y8.Instance.AwardAchievementAsync(
            randomAchievement.achievement,
            randomAchievement.achievementkey,
            false,
            false
        );
        LogResponse(response);
    }

    // ── Leaderboards ──────────────────────────────────────────────────────────

    public async void ButtonGetLeaderboardsAsync()
    {
        JsResponse<ScoreTables> response = await Y8.Instance.GetLeaderboardsAsync();
        LogResponse(response);
    }

    public async void ButtonGetLeaderboardScoresAsync()
    {
        JsResponse<ScoreTable> response = await Y8.Instance.GetLeaderboardScoresAsync(
            "test table",
            "alltime",
            20,
            1,
            true
        );
        LogResponse(response);
    }

    public async void ButtonShowLeaderboardAsync()
    {
        await Y8.Instance.ShowLeaderboardAsync("level_1", "alltime", true, false);
        LogDebug("[TestWrapper] Leaderboard closed");
    }

    public async void ButtonSaveLeaderboardScoreAsync()
    {
        int exampleScore = 3001 + UnityEngine.Random.Range(0, 1000);
        JsResponse<ScoreSave> response = await Y8.Instance.SaveLeaderboardScoreAsync(
            "level_1",
            exampleScore,
            false,
            true
        );
        LogResponse(response);
    }

    // ── Online Saves ──────────────────────────────────────────────────────────

    public async void ButtonSaveDataKeyAsync()
    {
        JsResponse<SetData> response = await Y8.Instance.SaveDataAsync("test_key", "monkey");
        LogResponse(response);
    }

    public async void ButtonSaveDataClassAsync()
    {
        JsResponse<SetData> response = await Y8.Instance.SaveDataAsync(
            "test_file_key",
            new SaveFileData
            {
                stringValue = "Test save",
                boolValue = true,
                floatValue = 0.01f
            }
        );
        LogResponse(response);
    }

    public async void ButtonLoadDataKeyAsync()
    {
        JsResponse<GetData> response = await Y8.Instance.LoadDataAsync("test_key");
        LogResponse(response);
    }

    public async void ButtonLoadDataClassAsync()
    {
        JsResponse<SaveFileData> response = await Y8.Instance.LoadDataAsync<SaveFileData>(
            "test_file_key"
        );
        LogResponse(response);

        if (response.IsSuccess && response.Data != null)
        {
            LogDebug(
                $"[TestWrapper] Loaded data: {response.Data.stringValue} | "
                    + $"{response.Data.boolValue} | {response.Data.floatValue}"
            );
        }
    }

    public async void ButtonRemoveDataAsync()
    {
        JsResponse<SetData> response = await Y8.Instance.RemoveDataAsync("test_file_key");
        //<SetData> response = await Y8.Instance.RemoveDataAsync("test_key");
        LogResponse(response);
    }

    // ── App Image ─────────────────────────────────────────────────────────────

    public async void ButtonSubmitImageAsync()
    {
        Texture2D screenshotTexture = null;
        StartCoroutine(TakeScreenshotCoroutine());

        while (screenshotTexture == null)
        {
            await Task.Yield();
        }

        JsResponse<SavedScreenshot> response = await Y8.Instance.SubmitImageAsync(
            screenshotTexture
        );
        if (response.IsSuccess)
        {
            LogDebug($"[TestWrapper] Image saved: {response.Data.imageUrl}");
        }

        IEnumerator TakeScreenshotCoroutine()
        {
            yield return new WaitForEndOfFrame();
            screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();
        }
    }

    public void ButtonGetUserValues()
    {
        LogDebug(
            "logged in="
                + Y8.Instance.IsLoggedIn()
                + " nickname="
                + Y8.Instance.Nickname()
                + " first name="
                + Y8.Instance.FirstName()
                + " token="
                + Y8.Instance.SessionToken()
                + " pid="
                + Y8.Instance.PID()
                + " date of birth="
                + Y8.Instance.DateOfBirth()
                + " gender="
                + Y8.Instance.Gender()
                + " language="
                + Y8.Instance.Language()
                + " locale="
                + Y8.Instance.Locale()
        );
    }

    public async void ButtonIsBlacklistedAsync()
    {
        JsResponse<bool> response = await Y8.Instance.IsBlacklistedAsync();
        LogDebug($"Is Success: {response.IsSuccess}, Is Blacklisted: {response.Data}");
    }

    public async void ButtonGetPlatformLocaleAsync()
    {
        JsResponse<PlatformLocale> response = await Y8.Instance.GetPlatformLocaleAsync();
        LogDebug($"Is Success: {response.IsSuccess}, Platform Locale: {response.Data?.locale}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void LogResponse<T>(JsResponse<T> response) =>
        LogDebug(
            $"[TestWrapper] IsSuccess={response.IsSuccess} "
                + $"Data={JsonUtility.ToJson(response.Data)}"
        );

    private void LogDebug(string info)
    {
        Debug.Log(info);
        debugText.text = info;
    }
}
