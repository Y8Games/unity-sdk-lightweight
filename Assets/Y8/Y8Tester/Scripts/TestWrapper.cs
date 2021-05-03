using UnityEngine;
using UnityEngine.UI;



/**
 * Starting a new project using the Combined Y8 Login SDK.
 * 
 * Add a new GameObject to your Scene then Add Component: Y8 (Script)
 * You can now access all of the public Y8 methods through the static Instance parameter.
 * 
 * e.g.
 *       Y8.Instance.Login((id, obj, failed) =>
 *          {
 *              if (failed)
 *                  Debug.Log("login failed!");
 *              else
 *                  Debug.Log("login succeeded! " + (obj != null ? obj.GetType().ToString(): "null"));
 *          });
 * 
 **/


/**
 * Test code to illustrate the different functions available in the combined Y8 Login SDK
 * The combined SDK will use the old Unity SDK for development in Unity Editor and will
 * switch to use the JS SDK automatically when built for WebGL targets.
 * 
 * This gives us the ability to quickly test from the Editor, but to use the more fully
 * featured and maintained JS SDK for game releases.
 * 
 * Not every feature of the JS SDK is available in the Unity SDK, notably:
 * Tables = Retrieve a list of highscore table titles.
 * Custom Score = Returns score data for custom high score menus.
 * App Request = Suggest this game to another Y8 user.
 * Friend Request = Offer a friend invite to another Y8 user.
 * Share = Share data with another Y8 user.
 * 
 * There are two different methods to obtain a response from the SDK which are shown in
 * functions ButtonAutoLogin() and ButtonLogin()
 * 
 * Take a look at ButtonGetInstantValues() for a list of methods to return useful values
 * instantly (after Login).
 * 
 * The generic 'object' returned in each callback represents an appropriate data class
 * from Y8_Data.cs which corresponds to the request which is being responded to.
 * e.g. a Login request generates a response which is decoded into a JS_Authorisation
 * object.  This only works in release builds (WebGL, not Unity Editor) due to the huge
 * differences between the Unity SDK data structures and the JS data.
 * Normally this should not be required, the instant access methods provide all commonly
 * used data fields in a platform independent way.
 * If deeper structures are required, you may need to branch your code using
 * #ifdef UNITY_EDITOR and obtain the values differently for your Unity Editor testing.
 * See the SDK documentation for Unity and JS here: https://docs.y8.com/
 * 
 * If you believe a commonly used feature is missing from this combined SDK, please
 * email Pete Baron at sibaroni@hotmail.com with the feature name and a brief description
 * of how you would prefer to access it.
 * 
 **/

public class TestWrapper : MonoBehaviour
{
    // debug only: output the progress and return values on screen so that
    // we can verify both Unity SDK and JS SDK work properly.
    public Text statusText;

    private int callId;



    private void Start()
    {
#if UNITY_EDITOR
        // disable buttons for features which are not availble in the Unity SDK
        GameObject go = GameObject.Find("Button_Tables");
        go.SetActive(false);
        go = GameObject.Find("Button_Custom_Score");
        go.SetActive(false);
        go = GameObject.Find("Button_App_Request");
        go.SetActive(false);
        go = GameObject.Find("Button_Friend_Request");
        go.SetActive(false);
        go = GameObject.Find("Button_Share");
        go.SetActive(false);
#endif
    }


    //
    // button clicks call Y8 wrapper functions
    //

    public void ButtonAutoLogin()
    {
        // first example: use a callback function
        // each call is identified by a unique ID
        callId = Y8.Instance.AutoLogin(loginCompleted);
        Debug.Log("AutoLogin with callback function.  Call ID = " + callId.ToString());
    }


    public void ButtonLogin()
    {
        // second example: embed the callback into the call as a lambda expression
        // the call ID is not needed in this case
        Y8.Instance.Login((id, obj, failed) =>
            {
                if (failed)
                    Debug.Log("login failed!");
                else
                    Debug.Log("login succeeded! " + (obj != null ? obj.GetType().ToString(): "null"));
            });
        Debug.Log("Called login with Lambda Expression callback");
    }


    public void ButtonRegister()
    {
        callId = Y8.Instance.Register(registerCompleted);
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonAchievementList()
    {
        callId = Y8.Instance.AchievementList(genericCompleted);
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonAchievementSave()
    {
        callId = Y8.Instance.AchievementSave(genericCompleted, "arg 2", "1f8c317c550bd6c1a0e2", false, false);
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonTables()
    {
        callId = Y8.Instance.Tables(genericCompleted);
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonCustomScore()
    {
        callId = Y8.Instance.CustomScore(genericCompleted, "test table", "alltime", 20, 1, true);
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonScoreList()
    {
        callId = Y8.Instance.ScoreList(genericCompleted, "test table", "alltime", true, false);
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonScoreSave()
    {
        int exampleScore = 3001 + UnityEngine.Random.Range(0, 1000);
        callId = Y8.Instance.ScoreSave(genericCompleted, "test table", exampleScore, false, true, "Test Player Name");
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonAppRequest()
    {
        callId = Y8.Instance.AppRequest(genericCompleted, "Play with me!", "https://y8.com", "");
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonFriendRequest()
    {
        callId = Y8.Instance.FriendRequest(genericCompleted, "574da07ee694aa5032001626", "http://id.net/");
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonShare()
    {
        callId = Y8.Instance.Share(genericCompleted, "<LINK URI>", "<DESCRIPTION>", "<NAME>", "<CAPTION>", "<PICTURE URI>");
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonSetData()
    {
        callId = Y8.Instance.SetData((id, obj, failed) =>
            {
                if (failed) Debug.Log("Y8 call failed! id = " + id.ToString());
                else Debug.Log("Y8 call succeeded! id = " + id.ToString());
            },
            "test_key", "monkey");
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonGetData()
    {
        callId = Y8.Instance.GetData(genericCompleted, "test_key");
        Debug.Log("Call ID = " + callId.ToString());
    }


    public void ButtonClearData()
    {
        callId = Y8.Instance.ClearData(genericCompleted, "test_key");
        Debug.Log("Call ID = " + callId.ToString());
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
        statusText.text += "\n" + debug;
    }


    //
    // callback functions when wrapper completes an action
    //

    private void loginCompleted(int id, object obj, bool failed)
    {
        if (failed) Debug.Log("login failed! id = " + id.ToString());
        else Debug.Log("login succeeded! id = " + id.ToString());
    }


    private void registerCompleted(int id, object obj, bool failed)
    {
        if (failed) Debug.Log("register user failed! id = " + id.ToString());
        else Debug.Log("register user succeeded! id = " + id.ToString());
    }


    private void genericCompleted(int id, object obj, bool failed)
    {
        if (failed) Debug.Log("Y8 call failed! id = " + id.ToString());
        else Debug.Log("Y8 call succeeded! id = " + id.ToString());
    }

}
