using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Y8API;

public class TestWrapper : MonoBehaviour
{
    public async void ButtonAutoLogin()
    {
        var response = await Y8.Instance.AutoLoginAsync();
        LogResponse(response);
    }

    public async void ButtonLogin()
    {
        var response = await Y8.Instance.LoginAsync();
        LogResponse(response);
    }

    public async void ButtonRegister()
    {
        var response = await Y8.Instance.RegisterAsync();
        LogResponse(response);
    }

    public async void ButtonShowAd()
    {
        await Y8.Instance.ShowAdAsync();
        Debug.Log("Ad finished");
    }

    public async void ButtonAchievementList()
    {
        await Y8.Instance.ShowAchievementListAsync();
        Debug.Log("Achievement list finished");
    }

    public async void ButtonAchievementSave()
    {
        var response = await Y8.Instance.SaveAchievementAsync("TestAchievement", "67ca8e11e839cd902960", false, false);
        LogResponse(response);
    }

    public async void ButtonTables()
    {
        var response = await Y8.Instance.GetTableNamesAsync();
        LogResponse(response);
    }

    public async void ButtonCustomScore()
    {
        var response = await Y8.Instance.GetCustomScoreAsync("test table", "alltime", 20, 1, true);
        LogResponse(response);
    }

    public async void ButtonScoreList()
    {
        await Y8.Instance.ShowScoreListAsync("test table", "alltime", true, false);
        Debug.Log("Score list finished");
    }

    public async void ButtonScoreSave()
    {
        int exampleScore = 3001 + Random.Range(0, 1000);
        var response = await Y8.Instance.SaveScoreAsync("test table", exampleScore, false, true);
        LogResponse(response);
    }

    public async void ButtonAppRequest()
    {
        await Y8.Instance.AppRequestAsync("Play with me!", "https://y8.com", "");
        Debug.Log("App request finished");
    }

    public async void ButtonFriendRequest()
    {
        await Y8.Instance.FriendRequestAsync("574da07ee694aa5032001626", "http://id.net/");
        Debug.Log("Friend request finished");
    }

    public async void ButtonSetData()
    {
        var response = await Y8.Instance.SetDataAsync("test_key", "monkey");
        LogResponse(response);
    }

    public async void ButtonGetData()
    {
        var response = await Y8.Instance.GetDataAsync("test_key");
        LogResponse(response);
    }

    public async void ButtonClearData()
    {
        var response = await Y8.Instance.ClearDataAsync("test_key");
        LogResponse(response);
    }

    public async void ButtonIsSponsor()
    {
        var response = await Y8.Instance.IsSponsorAsync();
        Debug.Log($"Is Success: {response.IsSuccess}, Is Sponsor: {response.Data}");
    }

    public async void ButtonIsBlacklisted()
    {
        var response = await Y8.Instance.IsBlacklistedAsync();
        Debug.Log($"Is Success: {response.IsSuccess}, Is Blacklisted: {response.Data}");
    }

    public async void TakeScreenshot()
    {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        Texture2D screenshotTexture = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        StartCoroutine(TakeScreenshotCoroutine());

        while (screenshotTexture == null) await Task.Yield();
        var response = await Y8.Instance.SaveScreenshotAsync(screenshotTexture);
        
        if (response.IsSuccess)
        {
            Debug.Log($"Screenshot saved to {response.Data.image}");
        }

        // WebGL clears every frame, needs to be triggered at EndOfFrame. Local method is used to keep the async/await
        IEnumerator TakeScreenshotCoroutine()
        {
            yield return new WaitForEndOfFrame();
            screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();            
        }
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
