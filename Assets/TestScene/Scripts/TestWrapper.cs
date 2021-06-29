using UnityEngine;
using UnityEngine.UI;

public class TestWrapper : MonoBehaviour
{
    public void ButtonAutoLogin()
    {
        Y8.Instance.AutoLogin(LoginCompleted);
    }

    public void ButtonLogin()
    {
        Y8.Instance.Login((id, obj, isSuccess) =>
            {
                if (isSuccess)
                {
                    Debug.Log("login succeeded! " + (obj != null ? obj.GetType().ToString() : "null"));
                }
                else
                {
                    Debug.Log("login failed!");
                }
            });
        Debug.Log("Called login with Lambda Expression callback");
    }

    public void ButtonRegister()
    {
        Y8.Instance.Register(RegisterCompleted);
    }

    public void ButtonShowAd()
    {
        Y8.Instance.ShowAd((id, obj, failed) => Debug.Log("Continue here"));
    }

    public void ButtonAchievementList()
    {
        Y8.Instance.AchievementList(GenericCompleted);
    }

    public void ButtonAchievementSave()
    {
        Y8.Instance.AchievementSave(GenericCompleted, "TestAchievement", "67ca8e11e839cd902960", false, false);
    }

    public void ButtonTables()
    {
        Y8.Instance.Tables(GenericCompleted);
    }

    public void ButtonCustomScore()
    {
        Y8.Instance.CustomScore(GenericCompleted, "test table", "alltime", 20, 1, true);
    }

    public void ButtonScoreList()
    {
        Y8.Instance.ScoreList(GenericCompleted, "test table", "alltime", true, false);
    }

    public void ButtonScoreSave()
    {
        int exampleScore = 3001 + Random.Range(0, 1000);
        Y8.Instance.ScoreSave(GenericCompleted, "test table", exampleScore, false, true, "Test Player Name");
    }

    public void ButtonAppRequest()
    {
        Y8.Instance.AppRequest(GenericCompleted, "Play with me!", "https://y8.com", "");
    }

    public void ButtonFriendRequest()
    {
        Y8.Instance.FriendRequest(GenericCompleted, "574da07ee694aa5032001626", "http://id.net/");
    }

    public void ButtonSetData()
    {
        Y8.Instance.SetData((id, obj, isSuccess) =>
            {
                if (isSuccess)
                {
                    Debug.Log("Y8 call succeeded! id = " + id.ToString());
                }
                else
                {
                    Debug.Log("Y8 call failed! id = " + id.ToString());
                }
            },
            "test_key", "monkey");
    }

    public void ButtonGetData()
    {
        Y8.Instance.GetData(GenericCompleted, "test_key");
    }

    public void ButtonClearData()
    {
        Y8.Instance.ClearData(GenericCompleted, "test_key");
    }

    public void ButtonGetInstantValues()
    {
        string debug =
            "logged in=" + Y8.Instance.LoggedIn().ToString() +
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

    public void ButtonIsSponsor()
    {
        Y8.Instance.IsSponsor((id, obj, isSuccess) => { Debug.Log($"Success: {isSuccess}, is sponsor: {(bool)obj}"); });
    }

    public void ButtonIsBlacklisted()
    {
        Y8.Instance.IsBlacklisted((id, obj, isSuccess) => { Debug.Log($"Success: {isSuccess}, is blacklisted: {(bool)obj}"); });
    }

    //
    // callback functions when wrapper completes an action
    //

    private void LoginCompleted(int id, object obj, bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("login succeeded! id = " + id.ToString());
        }
        else
        {
            Debug.Log("login failed! id = " + id.ToString());
        }
    }

    private void RegisterCompleted(int id, object obj, bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("register user succeeded! id = " + id.ToString());
        }
        else
        {
            Debug.Log("register user failed! id = " + id.ToString());
        }
    }

    private void GenericCompleted(int id, object obj, bool isSuccess)
    {
        if (isSuccess)
        {
            Debug.Log("Y8 call succeeded! id = " + id.ToString());
        }
        else
        {
            Debug.Log("Y8 call failed! id = " + id.ToString());
        }
    }
}
