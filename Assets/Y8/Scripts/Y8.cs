using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;



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

//
// Wrapper around Y8 Login system for Unity Development
//
// Relevant files are:
// Y8.cs - the wrapper functions and helpers
// Y8_Data.cs - data classes for JS responses
// LoginY8.jslib - JavaScript plugin that interfaces with the JS SDK
// 
// There are examples of all function calls in the file
// TestWrapper.js using button handlers to invoke everything.
//
// The basic idea is to wrap both the Unity SDK and provide
// access to the JS SDK through one set of easy to use
// functions.  The Unity SDK will be built when testing in
// the Unity Editor, but for release builds we switch to
// use the JS SDK which will be seen by the players.
//
// The reason for needing both is that the JS SDK cannot be
// accessed from the UnityEditor, and WebGL takes too long
// to build for regular code & test development cycles.
// The request to provide access to the JS SDK is motivated
// by a desire to reduce the number of duplicate user-
// interfaces that need to be maintained.
// 
// It will still be necessary to maintain the Unity SDK in
// working order, and to update it with new features as
// they are added to the JS SDK... however this task will
// be easier because we no longer need to make it look
// pretty for users.  It can have a bare minimum functional
// UI and still serve its reduced purpose for testing code.
//
// Provided that users do not reference any Idnet features
// directly, the Unity compiler should remove all Unity SDK
// code from the WebGL builds, nicely avoiding file bloat.
//


public class Y8 : MonoBehaviour
{
    // debug only: output the progress and return values on screen so that
    // we can verify both Unity SDK and JS SDK work properly.
    //public Text statusText = null;

    // this is the appid for "RealTimeTest" which has a convenient Highscore Table to test with
    // called "Example Project Scores"
    public static string AppId = "6081e298b4439c187a4a7e83";

    // has the Idnet system completed initialisation yet
    private static bool ready = false;
    private static int id = 10000;


#if !UNITY_EDITOR
    // bindings for JS functions in LoginY8.jslib
    [DllImport("__Internal")]
    private static extern void Init(string _AppId);
    [DllImport("__Internal")]
    private static extern void Call(int _id, string _request, string _jsonData);
    [DllImport("__Internal")]
    private static extern void Test(string _testScript);

    // we cannot pass the callback Action through JS, so we store it here
    // NOTE: this requires that only one async operation be in progress at
    // a time.  There should be a better solution...
    // the 'better solution' would probably involve creating an id for each async operation and passing that through the JS... then on callback it would know which operation the callback was for
    private Dictionary<int, Action<int, object, bool>> callCompleted = null;
    private JS_Authorisation auth;
#endif

    // MonoBehaviour does not allow proper Singleton class definition, this is a simplistic alternative.
    public static Y8 Instance = null;


    void Awake()
    {
        if (!Instance)
        {
            ready = false;
            Instance = this;
            Debug.Log("Y8.Awake first time");
        }
        else
        {
            Debug.Log("Y8.Awake again");
        }

        DontDestroyOnLoad(Instance.gameObject);

        #if !UNITY_EDITOR
            // create a Dictionary to store id values with the corresponding callback, used for JS implementation
            // to substitute for the C# coroutine approach
            Y8.Instance.callCompleted = new Dictionary<int, Action<int, object, bool>>(10);
        #endif
    }


    // debug only... output messages about SDK progress to a debug text display widget
    //public static void SetDebugText(Text statusTextDebug)
    //{
    //    Y8.Instance.statusText = statusTextDebug;
    //}




    //
    // Simple Interface:
    // Call these methods to access Y8.com features through both the JavaScript inteface
    // and the Unity SDK interface when running in the Unity Editor.
    // The Action parameter will be called-back when the async processes have finished.
    //


    /// <summary>
    /// https://docs.y8.com/docs/javascript/auth-functions/
    /// </summary>
    public int AutoLogin(Action<int, object, bool> loginCompleted)
    {
        StartCoroutine(WhenReadyCall(++Y8.id, "auto_login", null, loginCompleted));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/auth-functions/
    /// Opens a menu showing fields needed to register a new user. If the user is already logged in, it will close the menu and return to the redirect URI or callback.
    /// </summary>
    public int Register(Action<int, object, bool> registerCompleted)
    {
        StartCoroutine(WhenReadyCall(++Y8.id, "register", null, registerCompleted));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/auth-functions/
    /// It is the same process as Register
    /// You may use the same callback and options for Register and Login
    /// </summary>
    public int Login(Action<int, object, bool> loginCompleted)
    {
        StartCoroutine(WhenReadyCall(++Y8.id, "login", null, loginCompleted));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// Will display all achievements. If a player is logged in, it will display which achievements have been unlocked.
    /// </summary>
    public int AchievementList(Action<int, object, bool> completed)
    {
        StartCoroutine(WhenReadyCall(++Y8.id, "achievement_list", null, completed));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// </summary>
    /// <param name="achievement">The title of the achievement. This must exactly match.</param>
    /// <param name="achievementkey">The unlock key generated from the achievements application page.This must also exactly match.</param>
    /// <param name="overwrite">(optional) (default: false) Allow players to unlock the same achievement more than once.</param>
    /// <param name="allowduplicates">(optional)(default: false) Allow players to unlock the same achievement and display them seperatly.</param>
    public int AchievementSave(Action<int, object, bool> completed, string achievement, string achievementkey, bool overwrite = false, bool allowduplicates = false)
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("achievement", achievement),
            new KeyValuePair<string, string>("achievementkey", achievementkey),
            new KeyValuePair<string, string>("overwrite", overwrite.ToString().ToLower()),
            new KeyValuePair<string, string>("allowduplicates", allowduplicates.ToString().ToLower())
        };
        StartCoroutine(WhenReadyCall(++Y8.id, "achievement_save", json, completed));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// Return an app’s tables to a callback. Useful only when table names are unknown.
    /// </summary>
    public int Tables(Action<int, object, bool> completed)
    {
        StartCoroutine(WhenReadyCall(++Y8.id, "tables", null, completed));
        return Y8.id;
    }

    /// <summary>
    /// IMPORTANT: this feature is not available in the Unity SDK, so it cannot be tested from the Unity Editor.
    /// To use this feature, it is necessary to build a WebGL version of your project which will call the JS SDK.
    /// https://docs.y8.com/docs/javascript/game-api/
    /// Returns score data for custom high score menus.
    /// </summary>
    /// <param name="table">The exact table name from the app’s high scores page at y8.com. This is also the menu title.</param>
    /// <param name="mode">A string that equals alltime, last30days, last7days, today, or newest.</param>
    /// <param name="perPage">(optinal) (default: 20) Number of results to show per page.Max 100.</param>
    /// <param name="page">(optinal) (default: 1) A number representing the paged results.</param>
    /// <param name="highest">(optional)(default: true) Set to false if a lower score is better.</param>
    /// <param name="playerid">(optional) A string representing the player’s id or pid. Used for getting the player’s score(s)</param>
    public int CustomScore(Action<int, object, bool> completed, string table, string mode = "alltime", int perPage = 20, int page = 1, bool highest = true, string playerid = "")
    {
        List<KeyValuePair<string, string>> json = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("table", table),
            new KeyValuePair<string, string>("mode", mode),
            new KeyValuePair<string, string>("perPage", perPage.ToString()),
            new KeyValuePair<string, string>("page", page.ToString()),
            new KeyValuePair<string, string>("highest", highest.ToString().ToLower())
        };
        if (playerid != "")
        {
            json.Add(new KeyValuePair<string, string>("playerid", playerid));
        }
        StartCoroutine(WhenReadyCall(++Y8.id, "custom_score", json.ToArray(), completed));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// Display the high scores menu.
    /// </summary>
    /// <param name="tableTitle">The exact table name from the app’s high scores page at y8.com. This is also the menu title.</param>
    /// <param name="mode">(optional) A string that equals alltime, last30days, last7days, today, or newest.</param>
    /// <param name="highest">(optional)(default: true) Set to false if a lower score is better.</param>
    /// <param name="useMilli">(optional)(default: false) Render scores in milliseconds.</param>
    public int ScoreList(Action<int, object, bool> completed, string tableTitle, string mode = "alltime", bool highest = true, bool useMilli = false)
    {
        List<KeyValuePair<string, string>> json = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("table", tableTitle),
            new KeyValuePair<string, string>("mode", mode),
            new KeyValuePair<string, string>("highest", highest.ToString().ToLower())
        };
        if (useMilli)
        {
            // NOTE: JS SDK will useMilli even if it is 'false', the only way to prevent this is to leave it undefined
            json.Add(new KeyValuePair<string, string>("useMilli", useMilli.ToString().ToLower()));
        }
        StartCoroutine(WhenReadyCall(++Y8.id, "score_list", json.ToArray(), completed));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// Saves a player score.
    /// </summary>
    /// <param name="table">The exact table name from the app’s high scores page at Y8.com.</param>
    /// <param name="points">A number representing the player’s score</param>
    /// <param name="allowduplicates">(optional) (default: false) Set to true if player’s can submit more than one score.</param>
    /// <param name="highest">(optional) (default: true) Set to false if a lower score is better.</param>
    /// <param name="playername">(optional) (default: Y8.com nickname) Set when player’s in-game username is not the Y8.com nickname</param>
    public int ScoreSave(Action<int, object, bool> completed, string table, int points, bool allowduplicates = false, bool highest = true, string playername = "")
    {
        List<KeyValuePair<string, string>> json = new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("table", table),
            new KeyValuePair<string, string>("points", points.ToString()),
            new KeyValuePair<string, string>("allowduplicates", allowduplicates.ToString().ToLower()),
            new KeyValuePair<string, string>("highest", highest.ToString().ToLower())
        };
        if (playername != "")
        {
            json.Add(new KeyValuePair<string, string>("playername", playername));
        }
        StartCoroutine(WhenReadyCall(++Y8.id, "score_save", json.ToArray(), completed));
        return Y8.id;
    }

    /// <summary>
    /// IMPORTANT: this feature is not available in the Unity SDK, so it cannot be tested from the Unity Editor.
    /// To use this feature, it is necessary to build a WebGL version of your project which will call the JS SDK.
    /// https://docs.y8.com/docs/javascript/app-request-dialog/
    /// This SDK provides the ability to send an application request to invite friends to your application.
    /// To find more info on how your application can process those requests: https://docs.y8.com/docs/api/reference/requests/
    /// </summary>
    /// <param name="message">Invitation string that will appear in recipient's activity feed after request is sent.</param>
    /// <param name="redirect_uri">The URL to redirect to after a person clicks an 'Accept' button in the activity feed.</param>
    /// <param name="data">Arbitrary string that can store some data for your application.It will not be seen by the user.</param>
    public int AppRequest(Action<int, object, bool> completed, string message = "<message>", string redirect_uri = "", string data = "")
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("method", "apprequests"),
            new KeyValuePair<string, string>("message", message),
            new KeyValuePair<string, string>("redirect_uri", redirect_uri),
            new KeyValuePair<string, string>("data", data)
        };
        StartCoroutine(WhenReadyCall(++Y8.id, "app_request", json, completed));
        return Y8.id;
    }

    /// <summary>
    /// IMPORTANT: this feature is not available in the Unity SDK, so it cannot be tested from the Unity Editor.
    /// To use this feature, it is necessary to build a WebGL version of your project which will call the JS SDK.
    /// https://docs.y8.com/docs/javascript/friend-request-dialog/
    /// Y8.com SDK provides the ability to send a friend request from your application
    /// </summary>
    /// <param name="redirect_uri">The URL to redirect to after a person clicks a button on the dialog.</param>
    /// <param name="id">PID of the user in the application.</param>
    public int FriendRequest(Action<int, object, bool> completed, string id, string redirect_uri = "")
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("method", "friends"),
            new KeyValuePair<string, string>("id", id),
            new KeyValuePair<string, string>("redirect_uri", redirect_uri)
        };
        StartCoroutine(WhenReadyCall(++Y8.id, "friend_request", json, completed));
        return Y8.id;
    }

    /// <summary>
    /// IMPORTANT: this feature is not available in the Unity SDK, so it cannot be tested from the Unity Editor.
    /// To use this feature, it is necessary to build a WebGL version of your project which will call the JS SDK.
    /// https://docs.y8.com/docs/javascript/share-dialog/
    /// This SDK provides the ability to share an activity from your application.
    /// This activity will appear on the User’s feed page and feeds of his followers.
    /// </summary>
    /// <param name="link">The link where the user will be redirected by clicking on the shared content</param>
    /// <param name="description">The description of the content to share</param>
    /// <param name="name">The title of the content to share. If the "link" parameter is sent as well, then the link URL will be replaced by a link with the "name" parameter as text and the "link" paramater as redirection URL.</param>
    /// <param name="caption">The caption of the link/name</param>
    /// <param name="picture">The picture of the content to share.Has to be an absolute URL.</param>
    public int Share(Action<int, object, bool> completed, string link, string description, string name = "", string caption = "", string picture = "")
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("method", "feed"),
            new KeyValuePair<string, string>("link", link),
            new KeyValuePair<string, string>("description", description),
            new KeyValuePair<string, string>("name", name),
            new KeyValuePair<string, string>("caption", caption),
            new KeyValuePair<string, string>("picture", picture),
        };
        StartCoroutine(WhenReadyCall(++Y8.id, "share", json, completed));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/online-saves/
    /// Save the value, for later retrieval using the key.
    /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
    /// If you want to save frequently, such as when a player changes a setting, the code must buffer and retry failed submits. Submitting data could fail if things are saved too freqently.
    /// </summary>
    /// <param name="key">The key (or name) to be stored.</param>
    /// <param name="value">The value to be stored for access with that key.</param>
    public int SetData(Action<int, object, bool> completed, string key, string value)
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("key", key),
            new KeyValuePair<string, string>("value", value)
        };
        StartCoroutine(WhenReadyCall(++Y8.id, "set_data", json, completed));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/online-saves/
    /// Retrieve the value saved using this key.
    /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
    /// </summary>
    /// <param name="key">The key (or name) to be retrieved.</param>
    public int GetData(Action<int, object, bool> completed, string key)
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("key", key)
        };
        StartCoroutine(WhenReadyCall(++Y8.id, "get_data", json, completed));
        return Y8.id;
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/online-saves/
    /// Remove a key/value pair from the saved data.
    /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
    /// </summary>
    /// <param name="key">The key (or name) to be deleted with its corresponding value.</param>
    public int ClearData(Action<int, object, bool> completed, string key)
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("key", key)
        };
        StartCoroutine(WhenReadyCall(++Y8.id, "clear_data", json, completed));
        return Y8.id;
    }

    /// <summary>
    /// Check if the current URL is blacklisted
    /// </summary>
    /// <returns>true if it is</returns>
    public int IsBlacklisted(Action<int, object, bool> completed)
    {
        StartCoroutine(WhenReadyCall(++Y8.id, "blacklist", null, completed));
        return Y8.id;
    }

    /// <summary>
    /// Check if the current URL is a sponsor
    /// </summary>
    /// <returns>true if it is</returns>
    public int IsSponsor(Action<int, object, bool> completed)
    {
        StartCoroutine(WhenReadyCall(++Y8.id, "sponsor", null, completed));
        return Y8.id;
    }


    ///
    /// Quick access methods to immediately return values (often acquired in the user authentication response)
    ///


    /// <summary>
    /// true if the user is logged in to Y8.com
    /// </summary>
    public bool LoggedIn()
    {
#if UNITY_EDITOR
        // return Idnet.User.LoggedIn();
#else
            return auth != null
                && auth.authResponse != null
                && auth.authResponse.details != null
                && !string.IsNullOrEmpty(auth.authResponse.details.pid);
#endif
        return false;
    }

    /// <summary>
    /// the Unity SDK SessionToken or the JS SDK access_token or null if not authenticated yet
    /// </summary>
    public string SessionToken()
    {
#if UNITY_EDITOR
        // if (Idnet.User.Current != null)
        //     return Idnet.User.Current.SessionToken;
#else
            if (auth != null
                && auth.authResponse != null)
                return auth.authResponse.access_token;
#endif
        return null;
    }

    /// <summary>
    /// the player's PID or null if not authenticated yet
    /// </summary>
    public string PID()
    {
#if UNITY_EDITOR
        // if (Idnet.User.Current != null)
        //     return Idnet.User.Current.Pid;
#else
            if (auth != null
                && auth.authResponse != null
                && auth.authResponse.details != null)
                return auth.authResponse.details.pid;
#endif
        return null;
    }

    /// <summary>
    /// the player's first name or null if not authenticated yet
    /// </summary>
    public string FirstName()
    {
#if UNITY_EDITOR
        // if (Idnet.User.Current != null)
        //     return Idnet.User.Current.Firstname;
#else
            if (auth != null
                && auth.authResponse != null
                && auth.authResponse.details != null)
                return auth.authResponse.details.first_name;
#endif
        return null;
    }

    /// <summary>
    /// the player's nickname or null if not authenticated yet
    /// </summary>
    public string Nickname()
    {
#if UNITY_EDITOR
        // if (Idnet.User.Current != null)
        //     return Idnet.User.Current.Nickname;
#else
            if (auth != null
                && auth.authResponse != null
                && auth.authResponse.details != null)
                return auth.authResponse.details.nickname;
            else
                Debug.Log("Y8.Nickname: no auth response details!");
#endif
        return null;
    }

    /// <summary>
    /// the player's date of birth or null if not authenticated yet
    /// NOTE: in Unity Editor the day will always be '1'
    /// format is Y-M-D
    /// </summary>
    public string DateOfBirth()
    {
#if UNITY_EDITOR
        // if (Idnet.User.Current != null)
        //     return Idnet.User.Current.BirthYear + "-" + Idnet.User.Current.BirthMonth + "-1";
#else
            if (auth != null
                && auth.authResponse != null
                && auth.authResponse.details != null)
                return auth.authResponse.details.dob;
#endif
        return null;
    }

    /// <summary>
    /// the player's gender or null if not authenticated yet
    /// </summary>
    public string Gender()
    {
#if UNITY_EDITOR
        // if (Idnet.User.Current != null)
        //     return Idnet.User.Current.Gender;
#else
            if (auth != null
                && auth.authResponse != null
                && auth.authResponse.details != null)
                return auth.authResponse.details.gender;
#endif
        return null;
    }

    /// <summary>
    /// the player's language or null if not authenticated yet
    /// NOTE: always 'en-us' when testing in Editor
    /// </summary>
    public string Language()
    {
#if UNITY_EDITOR
        return "en-us";             // is it possible to get this from Unity SDK?
#else
            if (auth != null
                && auth.authResponse != null
                && auth.authResponse.details != null)
                return auth.authResponse.details.language;
            return null;
#endif
    }

    /// <summary>
    /// the player's locale or null if not authenticated yet
    /// NOTE: always 'en' when testing in Editor
    /// </summary>
    public string Locale()
    {
#if UNITY_EDITOR
        return "en";                // is it possible to get this from Unity SDK?
#else
            if (auth != null
                && auth.authResponse != null
                && auth.authResponse.details != null)
                return auth.authResponse.details.locale;
            return null;
#endif
    }



    //
    // general helpers for the SDK feature calls
    //

    private IEnumerator WhenReadyCall(int _id, string _request, KeyValuePair<string, string>[] _kvList, Action<int, object, bool> _callCompleted)
    {
        // if Idnet has not been initialised yet, do it now before calling anything else
        if (!ready)
        {
            // use the appropriate idnet SDK to initialise then wait for it to complete
            #if UNITY_EDITOR
                // Idnet.I.Initialize(Y8.AppId, false);
                // yield return Playtomic.Api.IsInitialized;
                // ready = true;
            #else
                Init(Y8.AppId);
                while (!ready) yield return null;
            #endif
        }

        Debug.Log("ready for [" + _id + "] '" + _request + "'");

        // use the appropriate helper to call the desired SDK feature
        // callback to _callCompleted when the async has completed
        #if UNITY_EDITOR
            CallUnity(_id, _request, _kvList, _callCompleted);
#else
            callCompleted.Add(_id, _callCompleted);
            string json = Y8.ConvertListToJson(_kvList);
            Debug.Log("JS call with JSON = " + json);
            Call(_id, _request, json);
#endif
        yield break;
    }


    private static string ConvertListToJson(KeyValuePair<string, string>[] _kvList)
    {
        if (_kvList == null) return "";

        // output format is:
        // { "key1":"value1 string", "key2":"value2 \"second\" string" }
        string s = "{ ";
        for (int i = 0, l = _kvList.Length; i < l; i++)
        {
            // we wrap the Value in quotes, so we have to escape all the quotes it contains
            s += '"' + _kvList[i].Key + '"'
            + ":"
            + '"' + _kvList[i].Value.Replace("\"", "\\" + "\"") + '"'
            + ((i < l - 1) ? ", " : "");
        }
        s += " }";

        return s;
    }


#if UNITY_EDITOR

    //
    // use the Unity SDK instead of LoginY8.jslib when testing in the Unity Editor
    //

    private static void CallUnity(int _id, string _request, KeyValuePair<string, string>[] _kvList, Action<int, object, bool> _callCompleted)
    {
        Debug.Log(_id.ToString() + " '" + _request + "' with data: " + Y8.ConvertListToJson(_kvList));
        string s;
        object o;

        // call the appropriate ID function
        switch (_request)
        {
            case "auto_login":
                // Idnet.I.AutoLogin((user, exception) =>
                // {
                //     bool failed = true;
                //     if (exception != null)
                //     {
                //         //Y8.Instance.statusText.text += "Auto login: failed. User is not logged-in or we're running in Unity Editor. " + exception;
                //     }
                //     else
                //     {
                //         if (Idnet.User.LoggedIn())
                //         {
                //             //Y8.Instance.statusText.text += "Auto login: success. " + "Nickname " + Idnet.User.Current.Nickname;
                //             failed = false;
                //         }
                //         else
                //         {
                //             //Y8.Instance.statusText.text += "Auto login: no exception but user is not logged in";
                //         }
                //     }
                //     _callCompleted(_id, exception, failed);
                // });
                break;

            case "login":
                // Idnet.I.Login(exception =>
                // {
                //     bool failed = true;
                //     if (Idnet.User.LoggedIn())
                //     {
                //         // Callback: Login Success,you will get user's nickname in callback.
                //         //Y8.Instance.statusText.text += "User logged in: " + "Nickname " + Idnet.User.Current.Nickname;
                //         failed = false;
                //     }
                //     _callCompleted(_id, exception, failed);
                // });
                break;

            case "register":
                // Idnet.I.Register(exception =>
                // {
                //     bool failed = true;
                //     if (Idnet.User.LoggedIn())
                //     {
                //         // Callback: Register & hence Login Success,you will get user's nickname in callback.
                //         //Y8.Instance.statusText.text += "User logged in: " + "Nickname " + Idnet.User.Current.Nickname;
                //         failed = false;
                //     }
                //     _callCompleted(_id, exception, failed);
                // });
                break;

            case "achievement_list":
                // Idnet.I.ListAchievements(exception =>
                // {
                //     //Y8.Instance.statusText.text += "List Achievements " + exception;
                //     _callCompleted(_id, null, false);
                // });
                break;

            case "achievement_save":
                // "{ achievement: \"arg 2\", achievementkey: \"1f8c317c550bd6c1a0e2\", overwrite: false, allowduplicates: false }";
                // Idnet.AchievementUnlocker au = new Idnet.AchievementUnlocker(_kvList[0].Value, _kvList[1].Value);
                // Idnet.I.UnlockAchievement(au, exception =>
                // {
                //     bool failed = true;
                //     if (exception == null)
                //     {
                //         //Y8.Instance.statusText.text += "Achievement unlocked sucessfully: " + "\n" + au.Name + "\n" + au.Key;
                //         failed = false;
                //     }
                //     else
                //     {
                //         //Y8.Instance.statusText.text += exception;
                //     }
                //     _callCompleted(_id, exception, failed);
                // });
                break;

            case "score_list":
                // "{ table: \"test table\", mode: \"alltime\", highest: false }";
                // Idnet.I.Leaderboard(_kvList[0].Value, (list, exception) =>
                // {
                //     bool failed = true;
                //     if (exception == null)
                //     {
                //         //Y8.Instance.statusText.text += "Score List success.";
                //         failed = false;
                //     }
                //     else
                //     {
                //         //Y8.Instance.statusText.text += "Score List failed. " + exception;
                //     }
                //     _callCompleted(_id, exception, failed);
                // }, null, Idnet.LeaderboardMode.AllTime);
                break;

            case "score_save":
                // "{ points: " + exampleScore.ToString() + ", highest: true, table: \"test table\", allowduplicates: false }";
                // Idnet.I.PostHighscore(int.Parse(_kvList[1].Value), _kvList[0].Value, (value, exception) =>
                // {
                //     bool failed = true;
                //     if (exception == null)
                //     {
                //         //Y8.Instance.statusText.text += "Score Save success. " + value.ToString();
                //         failed = false;
                //     }
                //     else
                //     {
                //         //Y8.Instance.statusText.text += "Score Save failed. " + exception;
                //     }
                //     _callCompleted(_id, exception, failed);
                // });
                break;

            case "set_data":
                // "{ key: \"test\", value: \"monkey\" }";
                s = _kvList[1].Value;
                // Idnet.I.Post<string>(_kvList[0].Value, s, (strng, exception) =>
                // {
                //     bool failed = true;
                //     if (exception == null)
                //     {
                //         Debug.Log("Data posted as '" + _kvList[0].Value + "' = '" + s + "'");
                //         failed = false;
                //     }
                //     else
                //     {
                //         //Y8.Instance.statusText.text += "Posting Data failed. " + exception;
                //     }
                //     _callCompleted(_id, exception, failed);
                // });
                break;

            case "get_data":
                // "{ key: \"test\" }";
                // Idnet.I.Get<string>(_kvList[0].Value, (keyValuePair, exception) =>
                // {
                //     bool failed = true;
                //     if (exception != null)
                //     {
                //         //Y8.Instance.statusText.text += "Get failed for '" + keyValuePair.Key + "'";
                //         _callCompleted(_id, exception, failed);
                //     }
                //     else
                //     {
                //         s = keyValuePair.Value.Replace("\\" + "\"", "\"");
                //         Debug.Log("Retrieved '" + keyValuePair.Key + "' with value '" + s + "'");
                //         failed = false;
                //         _callCompleted(_id, s, failed);
                //     }
                // });
                break;

            case "clear_data":
                // "{ key: \"test\" }";
                // Idnet.I.DeleteKey(_kvList[0].Value, (response, exception) =>
                // {
                //     bool failed = true;
                //     if (exception != null)
                //     {
                //         //Y8.Instance.statusText.text += "Clear Data failed " + exception.Message;
                //     }
                //     else
                //     {
                //         //Y8.Instance.statusText.text += "Cleared key " + response;
                //         failed = false;
                //     }
                //     _callCompleted(_id, exception, failed);
                // });
                break;

            case "blacklist":
                // s = Idnet.I.IsBlacklisted.ToString().ToLower();
                // o = (object)s;
                // _callCompleted(_id, o, false);
                break;

            case "sponsor":
                // s = Idnet.I.IsSponsor.ToString().ToLower();
                // o = (object)s;
                // _callCompleted(_id, o, false);
                break;

            /* there isn't a Unity equivalent for any of these JS features...
                        case "tables":
                            break;

                        case "custom_score":
                            break;

                        case "app_request":
                            break;

                        case "friend_request":
                            break;

                        case "share":
                            break;
            */
            default:
                Debug.LogWarning("There is no Unity feature to match '" + _request + "', it _might_ be valid in the JS SDK when building for WebGL");
                break;
        }
    }

#else

    //
    // JS callback functions to report status to Unity C#
    //

    // call-back from JS when the system has completed init
    public void CallbackReady()
    {
        Debug.Log("Y8 login system is ready.");
        ready = true;
    }

    // call-back from JS with the auth response
    public void AuthCallbackResponse(string _response)
    {
        int authId = id;    // id could change in the callback, remember its current value
        bool failed = true;

        Debug.Log("AuthResponse from JS: " + _response);

        auth = JsonUtility.FromJson<JS_Authorisation>(_response);
        if (!string.IsNullOrEmpty(auth.status))
        {
            //statusText.text += "\nReceived AuthResponse status = " + auth.status + " logged in = " + LoggedIn();
            failed = !LoggedIn();
        }
        else
        {
            Debug.Log("Unknown response to Authorisation Request! " + _response);
        }

        // callback when all is completed
        Action<int, object, bool> callback;
        if (callCompleted.TryGetValue(authId, out callback))
        {
            Debug.Log("perform callback for [" + authId.ToString() + "]");
            callback(authId, auth, failed);
            callCompleted.Remove(authId);
        }
        else
        {
            Debug.Log("There is no callback for auth response to id " + authId.ToString());
        }
    }

    // call-back to C# from JS with the response to a request packed into a Y8_Data class
    public void CallbackResponse(string _responseString)
    {
        // extract the response string components
        // the format is always <request string>[<id number>]=<response>
        Debug.Log(_responseString);

        // extract the id from square brackets
        int ob = _responseString.IndexOf("[");
        int cb = _responseString.IndexOf("]");
        string request = _responseString.Substring(0, ob);
        int _id = int.Parse(_responseString.Substring(ob + 1, cb - ob - 1));
        string response = _responseString.Substring(cb + 2);
        Debug.Log("Response from JS: " + request + "[" + _id + "] = '" + response + "'");

        bool failed = false;        // default value assume success
        object obj = response;      // default value to the response string
        string s;

        // parse the response JSON into the correct C# class object so it can be accessed easily
        switch (request)
        {
            case "achievement_save":
                JS_AchievementSave achSave = JsonUtility.FromJson<JS_AchievementSave>(response);
                obj = achSave;
                if (!achSave.success)
                {
                    failed = true;
                }
                break;

            case "score_save":
                JS_ScoreSave scoreSave = JsonUtility.FromJson<JS_ScoreSave>(response);
                obj = scoreSave;
                if (!string.IsNullOrEmpty(scoreSave.errormessage))
                {
                    failed = true;
                }
                else if (!scoreSave.success)
                {
                    failed = true;
                }
                break;

            case "set_data":
            case "clear_data":
                JS_SetData setData = JsonUtility.FromJson<JS_SetData>(response);
                obj = setData;
                if (!string.IsNullOrEmpty(setData.status))
                {
                    if (setData.status != "ok")
                        failed = true;
                }
                break;

            case "get_data":
                // get_data returns just the data, nothing else... (to match the Unity version)
                JS_GetData getData = JsonUtility.FromJson<JS_GetData>(response);
                s = getData.jsondata;
                // the content of the response from JS has a pair of quotes around it because it is saved as a string in a key/value pair, remove them
                if (s[0] == '\"' && s[s.Length - 1] == '\"')
                    s = s.Substring(1, s.Length - 2);
                // remove escape characters before the quotes embedded in the string
                obj = s.Replace("\\" + "\"", "\"");
                if (!string.IsNullOrEmpty(getData.error))
                {
                    failed = true;
                }
                break;

            case "custom_score":
                JS_ScoreTable getTable = JsonUtility.FromJson<JS_ScoreTable>(response);
                obj = getTable;
                if (getTable.errorcode != 0)
                {
                    failed = true;
                }
                break;

            case "tables":
                JS_ScoreTables getTables = JsonUtility.FromJson<JS_ScoreTables>(response);
                obj = getTables;
                if (getTables.errorcode != 0)
                {
                    failed = true;
                }
                break;

            case "blacklist":
            case "sponsor":
                break;

            default:
                Debug.Log("Unhandled request type: " + request);
                break;
        }

        // callback when all is completed
        Action<int, object, bool> callback;
        if (callCompleted.TryGetValue(_id, out callback))
        {
            Debug.Log("Callback for response to id " + _id.ToString());
            callback(_id, obj, failed);
            callCompleted.Remove(_id);
        }
        else
        {
            Debug.Log("There is no callback for response to id " + _id.ToString());
        }
    }

#endif

}       // end of class Y8
