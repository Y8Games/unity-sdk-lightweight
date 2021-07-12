using UnityEngine;
using Y8API;

public class TestWrapper : MonoBehaviour
{
    public async void ButtonAutoLogin()
    {
        var response = await Y8.Instance.AutoLogin();
        LogResponse(response);
    }

    public async void ButtonLogin()
    {
        var response = await Y8.Instance.Login();
        LogResponse(response);
    }

    public async void ButtonRegister()
    {
        var response = await Y8.Instance.Register();
        LogResponse(response);
    }

    public async void ButtonShowAd()
    {
        await Y8.Instance.ShowAd();
        Debug.Log("Ad finished");
    }

    public async void ButtonAchievementList()
    {
        await Y8.Instance.AchievementList();
        Debug.Log("Achievement list finished");
    }

    public async void ButtonAchievementSave()
    {
        var response = await Y8.Instance.AchievementSave("TestAchievement", "67ca8e11e839cd902960", false, false);
        LogResponse(response);
    }

    public async void ButtonTables()
    {
        var response = await Y8.Instance.Tables();
        LogResponse(response);
    }

    public async void ButtonCustomScore()
    {
        var response = await Y8.Instance.CustomScore("test table", "alltime", 20, 1, true);
        LogResponse(response);
    }

    public async void ButtonScoreList()
    {
        await Y8.Instance.ScoreList("test table", "alltime", true, false);
        Debug.Log("Score list finished");
    }

    public async void ButtonScoreSave()
    {
        int exampleScore = 3001 + Random.Range(0, 1000);
        var response = await Y8.Instance.ScoreSave("test table", exampleScore, false, true);
        LogResponse(response);
    }

    public async void ButtonAppRequest()
    {
        await Y8.Instance.AppRequest("Play with me!", "https://y8.com", "");
        Debug.Log("App request finished");
    }

    public async void ButtonFriendRequest()
    {
        await Y8.Instance.FriendRequest("574da07ee694aa5032001626", "http://id.net/");
        Debug.Log("Friend request finished");
    }

    public async void ButtonSetData()
    {
        var response = await Y8.Instance.SetData("test_key", "monkey");
        LogResponse(response);
    }

    public async void ButtonGetData()
    {
        var response = await Y8.Instance.GetData("test_key");
        LogResponse(response);
    }

    public async void ButtonClearData()
    {
        var response = await Y8.Instance.ClearData("test_key");
        LogResponse(response);
    }

    public async void ButtonIsSponsor()
    {
        var response = await Y8.Instance.IsSponsor();
        Debug.Log($"Is Success: {response.IsSuccess}, Is Sponsor: {response.Data}");
    }

    public async void ButtonIsBlacklisted()
    {
        var response = await Y8.Instance.IsBlacklisted();
        Debug.Log($"Is Success: {response.IsSuccess}, Is Blacklisted: {response.Data}");
    }

    public void ButtonGetInstantValues()
    {
        string debug =
            "logged in=" + Y8.Instance.IsLoggedIn().ToString() +
            " nickname=" + Y8.Instance.Nickname() +
            " first name=" + Y8.Instance.FirstName() +
            " token=" + Y8.Instance.SessionToken() +
            " pid=" + Y8.Instance.PID() +
            " date of birth=" + Y8.Instance.DateOfBirth() +
            " gender=" + Y8.Instance.Gender() +
            " language=" + Y8.Instance.Language() +
            " locale=" + Y8.Instance.Locale();
        Debug.Log(debug);
    }

    private void LogResponse<T>(JsResponse<T> response)
    {
        Debug.Log($"Is Success: {response.IsSuccess}, Data: {JsonUtility.ToJson(response.Data)}");
    }
}
