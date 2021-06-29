using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;

public class Y8 : MonoBehaviour
{
    // MonoBehaviour does not allow proper Singleton class definition, this is a simplistic alternative.
    public static Y8 Instance = null;

    private static bool isReady = false;

    [Header("ENTER APP ID HERE")] public string AppId = "";
    [Header("ENTER ADS ID HERE")] public string AdsId = "";

    private int id = 10000;

    // Jslib invokes methods on object with specific name > we need to enforce it
    private readonly string calleeName = "Y8_Root";

    // we cannot pass the callback Action through JS, so we store it here
    // NOTE: this requires that only one async operation be in progress at
    // a time.  There should be a better solution...
    // the 'better solution' would probably involve creating an id for each async operation and passing that through the JS... then on callback it would know which operation the callback was for

    private readonly Dictionary<int, Action<int, object, bool>> invokedCalls =
        new Dictionary<int, Action<int, object, bool>>();

    private JS_Authorisation auth;

    private void Awake()
    {
        if (Instance == null)
        {
            isReady = false;
            Instance = this;
            gameObject.name = calleeName;
            transform.SetParent(null);
            Init(AppId, AdsId);

            DontDestroyOnLoad(Instance.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR

    private static void Init(string appId, string adsId)
    {
        Debug.Log($"Y8.Init: {appId}, ads: {adsId}");
        isReady = true;
    }

    private static void Call(
    int _id,
    string _request,
    string _jsonData)
    {
        Debug.Log($"{_id} '{_request}' with data: {_jsonData}");
    }

#else
    // bindings for JS functions in Y8.jslib
    [DllImport("__Internal")]
    private static extern void Init(string _AppId, string _AdsId);
    [DllImport("__Internal")]
    private static extern void Call(int _id, string _request, string _jsonData);
#endif

    /// <summary>
    /// https://docs.y8.com/docs/javascript/auth-functions/
    /// </summary>
    public void AutoLogin(Action<int, object, bool> loginCallback)
    {
        TryCall("auto_login", null, loginCallback);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/auth-functions/
    /// Opens a menu showing fields needed to register a new user. If the user is already logged in, it will close the menu and return to the redirect URI or callback.
    /// </summary>
    public void Register(Action<int, object, bool> registerCallback)
    {
        TryCall("register", null, registerCallback);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/auth-functions/
    /// It is the same process as Register
    /// You may use the same callback and options for Register and Login
    /// </summary>
    public void Login(Action<int, object, bool> loginCompleted)
    {
        TryCall("login", null, loginCompleted);
    }

    public void ShowAd(Action<int, object, bool> showAdCompleted)
    {
        TryCall("show_ad", null, showAdCompleted);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// Will display all achievements. If a player is logged in, it will display which achievements have been unlocked.
    /// </summary>
    public void AchievementList(Action<int, object, bool> callback)
    {
        TryCall("achievement_list", null, callback);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// </summary>
    /// <param name="achievement">The title of the achievement. This must exactly match.</param>
    /// <param name="achievementkey">The unlock key generated from the achievements application page.This must also exactly match.</param>
    /// <param name="overwrite">(optional) (default: false) Allow players to unlock the same achievement more than once.</param>
    /// <param name="allowduplicates">(optional)(default: false) Allow players to unlock the same achievement and display them seperatly.</param>
    public void AchievementSave(Action<int, object, bool> callback, string achievement, string achievementkey, bool overwrite = false, bool allowduplicates = false)
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("achievement", achievement),
            new KeyValuePair<string, string>("achievementkey", achievementkey),
            new KeyValuePair<string, string>("overwrite", overwrite.ToString().ToLower()),
            new KeyValuePair<string, string>("allowduplicates", allowduplicates.ToString().ToLower())
        };
        TryCall("achievement_save", json, callback);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// Return an app’s tables to a callback. Useful only when table names are unknown.
    /// </summary>
    public void Tables(Action<int, object, bool> callback)
    {
        TryCall("tables", null, callback);
    }

    /// <summary>
    /// Returns score data for custom high score menus.
    /// </summary>
    /// <param name="table">The exact table name from the app’s high scores page at y8.com. This is also the menu title.</param>
    /// <param name="mode">A string that equals alltime, last30days, last7days, today, or newest.</param>
    /// <param name="perPage">(optinal) (default: 20) Number of results to show per page.Max 100.</param>
    /// <param name="page">(optinal) (default: 1) A number representing the paged results.</param>
    /// <param name="highest">(optional)(default: true) Set to false if a lower score is better.</param>
    /// <param name="playerid">(optional) A string representing the player’s id or pid. Used for getting the player’s score(s)</param>
    public void CustomScore(Action<int, object, bool> callback, string table, string mode = "alltime", int perPage = 20, int page = 1, bool highest = true, string playerid = "")
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
        TryCall("custom_score", json.ToArray(), callback);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/game-api/
    /// Display the high scores menu.
    /// </summary>
    /// <param name="tableTitle">The exact table name from the app’s high scores page at y8.com. This is also the menu title.</param>
    /// <param name="mode">(optional) A string that equals alltime, last30days, last7days, today, or newest.</param>
    /// <param name="highest">(optional)(default: true) Set to false if a lower score is better.</param>
    /// <param name="useMilli">(optional)(default: false) Render scores in milliseconds.</param>
    public void ScoreList(Action<int, object, bool> callback, string tableTitle, string mode = "alltime", bool highest = true, bool useMilli = false)
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
        TryCall("score_list", json.ToArray(), callback);
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
    public void ScoreSave(Action<int, object, bool> callback, string table, int points, bool allowduplicates = false, bool highest = true, string playername = "")
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
        TryCall("score_save", json.ToArray(), callback);
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
    public void AppRequest(Action<int, object, bool> callback, string message = "<message>", string redirect_uri = "", string data = "")
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("method", "apprequests"),
            new KeyValuePair<string, string>("message", message),
            new KeyValuePair<string, string>("redirect_uri", redirect_uri),
            new KeyValuePair<string, string>("data", data)
        };
        TryCall("app_request", json, callback);
    }

    /// <summary>
    /// IMPORTANT: this feature is not available in the Unity SDK, so it cannot be tested from the Unity Editor.
    /// To use this feature, it is necessary to build a WebGL version of your project which will call the JS SDK.
    /// https://docs.y8.com/docs/javascript/friend-request-dialog/
    /// Y8.com SDK provides the ability to send a friend request from your application
    /// </summary>
    /// <param name="redirect_uri">The URL to redirect to after a person clicks a button on the dialog.</param>
    /// <param name="id">PID of the user in the application.</param>
    public void FriendRequest(Action<int, object, bool> callback, string _id, string redirect_uri = "")
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("method", "friends"),
            new KeyValuePair<string, string>("id", _id),
            new KeyValuePair<string, string>("redirect_uri", redirect_uri)
        };
        TryCall("friend_request", json, callback);
    }

    /// <summary>    ///
    /// https://docs.y8.com/docs/javascript/share-dialog/
    /// This SDK provides the ability to share an activity from your application.
    /// This activity will appear on the User’s feed page and feeds of his followers.
    /// [Currently not available]
    /// </summary>
    /// <param name="link">The link where the user will be redirected by clicking on the shared content</param>
    /// <param name="description">The description of the content to share</param>
    /// <param name="name">The title of the content to share. If the "link" parameter is sent as well, then the link URL will be replaced by a link with the "name" parameter as text and the "link" paramater as redirection URL.</param>
    /// <param name="caption">The caption of the link/name</param>
    /// <param name="picture">The picture of the content to share.Has to be an absolute URL.</param>
    public void Share(Action<int, object, bool> callback, string link, string description, string name = "", string caption = "", string picture = "")
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("method", "feed"),
            new KeyValuePair<string, string>("link", link),
            new KeyValuePair<string, string>("description", description),
            new KeyValuePair<string, string>("name", name),
            new KeyValuePair<string, string>("caption", caption),
            new KeyValuePair<string, string>("picture", picture),
        };
        TryCall("share", json, callback);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/online-saves/
    /// Save the value, for later retrieval using the key.
    /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
    /// If you want to save frequently, such as when a player changes a setting, the code must buffer and retry failed submits. Submitting data could fail if things are saved too freqently.
    /// </summary>
    /// <param name="key">The key (or name) to be stored.</param>
    /// <param name="value">The value to be stored for access with that key.</param>
    public void SetData(Action<int, object, bool> callback, string key, string value)
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("key", key),
            new KeyValuePair<string, string>("value", value)
        };
        TryCall("set_data", json, callback);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/online-saves/
    /// Retrieve the value saved using this key.
    /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
    /// </summary>
    /// <param name="key">The key (or name) to be retrieved.</param>
    public void GetData(Action<int, object, bool> callback, string key)
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("key", key)
        };
        TryCall("get_data", json, callback);
    }

    /// <summary>
    /// https://docs.y8.com/docs/javascript/online-saves/
    /// Remove a key/value pair from the saved data.
    /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
    /// </summary>
    /// <param name="key">The key (or name) to be deleted with its corresponding value.</param>
    public void ClearData(Action<int, object, bool> callback, string key)
    {
        KeyValuePair<string, string>[] json = {
            new KeyValuePair<string, string>("key", key)
        };
        TryCall("clear_data", json, callback);
    }

    /// <summary>
    /// Check if the current URL is blacklisted
    /// </summary>
    /// <returns>true if it is</returns>
    public void IsBlacklisted(Action<int, object, bool> callback)
    {
        TryCall("blacklist", null, callback);
    }

    /// <summary>
    /// Check if the current URL is a sponsor
    /// </summary>
    /// <returns>true if it is</returns>
    public void IsSponsor(Action<int, object, bool> callback)
    {
        TryCall("sponsor", null, callback);
    }

    ///
    /// Quick access methods to immediately return values (often acquired in the user authentication response)
    ///

    /// <summary>
    /// true if the user is logged in to Y8.com
    /// </summary>
    public bool LoggedIn()
    {
        return auth != null
            && auth.authResponse != null
            && auth.authResponse.details != null
            && !string.IsNullOrEmpty(auth.authResponse.details.pid);
    }

    /// <summary>
    /// the Unity SDK SessionToken or the JS SDK access_token or null if not authenticated yet
    /// </summary>
    public string SessionToken()
    {
        if (auth != null && auth.authResponse != null)
        {
            return auth.authResponse.access_token;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// the player's PID or null if not authenticated yet
    /// </summary>
    public string PID()
    {
        if (auth != null
            && auth.authResponse != null
            && auth.authResponse.details != null)
        {
            return auth.authResponse.details.pid;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// the player's first name or null if not authenticated yet
    /// </summary>
    public string FirstName()
    {
        if (auth != null
            && auth.authResponse != null
            && auth.authResponse.details != null)
        {
            return auth.authResponse.details.first_name;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// the player's nickname or null if not authenticated yet
    /// </summary>
    public string Nickname()
    {
        if (auth != null
            && auth.authResponse != null
            && auth.authResponse.details != null)
        {
            return auth.authResponse.details.nickname;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// the player's date of birth or null if not authenticated yet
    /// NOTE: in Unity Editor the day will always be '1'
    /// format is Y-M-D
    /// </summary>
    public string DateOfBirth()
    {
        if (auth != null
            && auth.authResponse != null
            && auth.authResponse.details != null)
        {
            return auth.authResponse.details.dob;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// the player's gender or null if not authenticated yet
    /// </summary>
    public string Gender()
    {
        if (auth != null
            && auth.authResponse != null
            && auth.authResponse.details != null)
        {
            return auth.authResponse.details.gender;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// the player's language or null if not authenticated yet
    /// </summary>
    public string Language()
    {
        if (auth != null
            && auth.authResponse != null
            && auth.authResponse.details != null)
        {
            return auth.authResponse.details.language;
        }
        else
        {
            return CultureInfo.CurrentCulture.Name;
        }
    }

    /// <summary>
    /// the player's locale or null if not authenticated yet
    /// </summary>
    public string Locale()
    {
        if (auth != null
            && auth.authResponse != null
            && auth.authResponse.details != null)
        {
            return auth.authResponse.details.locale;
        }
        else
        {
            return CultureInfo.CurrentCulture.Name;
        }
    }

    //
    // general helpers for the SDK feature calls
    //

    private void TryCall(string requestName, KeyValuePair<string, string>[] kvPairs, Action<int, object, bool> callback)
    {
        if (!isReady)
        {
            Debug.Log("SDK is not ready");
            return;
        }

        id++;
        invokedCalls.Add(id, callback);
        string json = ConvertListToJson(kvPairs);
        Debug.Log($"JS call [{id}] with JSON = {json}");
        Call(id, requestName, json);
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

    //
    // JS callback functions to report status to Unity C#
    //

    // call-back from JS when the system has completed init
    public void CallbackReady()
    {
        Debug.Log("Y8 login system is ready.");
        isReady = true;
    }

    // call-back from JS with the auth response
    public void AuthCallbackResponse(string _response)
    {
        int authId = id;    // id could change in the callback, remember its current value
        bool failed = true;

        Debug.Log($"AuthResponse from JS: {_response} ");

        auth = JsonUtility.FromJson<JS_Authorisation>(_response);
        if (!string.IsNullOrEmpty(auth.status))
        {
            //statusText.text += "\nReceived AuthResponse status = " + auth.status + " logged in = " + LoggedIn();
            failed = !LoggedIn();
        }
        else
        {
            Debug.Log($"Unknown response to Authorisation Request! {_response}");
        }

        // callback when all is completed
        if (invokedCalls.TryGetValue(authId, out Action<int, object, bool> callback))
        {
            Debug.Log($"perform callback for [{authId}]");
            callback(authId, auth, failed);
            invokedCalls.Remove(authId);
        }
        else
        {
            Debug.Log($"There is no callback for auth response to id {authId}");
        }
    }

    // call-back to C# from JS with the response to a request packed into a Y8_Data class
    public void CallbackResponse(string _responseString)
    {
        // extract the response string components
        // the format is always <request string>[<id number>]=<response>

        // extract the id from square brackets
        int ob = _responseString.IndexOf("[");
        int cb = _responseString.IndexOf("]");
        string request = _responseString.Substring(0, ob);
        int _id = int.Parse(_responseString.Substring(ob + 1, cb - ob - 1));
        string response = _responseString.Substring(cb + 2);
        Debug.Log($"Response from JS: {request}[{_id}] = '{response}'");

        bool isSuccess = true;        // default value assume success
        object obj = response;      // default value to the response string

        // parse the response JSON into the correct C# class object so it can be accessed easily
        switch (request)
        {
            case "achievement_save":
                JS_AchievementSave achSave = JsonUtility.FromJson<JS_AchievementSave>(response);
                obj = achSave;
                isSuccess = achSave.success;
                break;

            case "score_save":
                JS_ScoreSave scoreSave = JsonUtility.FromJson<JS_ScoreSave>(response);
                obj = scoreSave;
                isSuccess = scoreSave.success;
                break;

            case "set_data":
            case "clear_data":
                JS_SetData setData = JsonUtility.FromJson<JS_SetData>(response);
                obj = setData;
                isSuccess = setData.status == "ok";
                break;

            case "get_data":
                // get_data returns just the data, nothing else... (to match the Unity version)
                JS_GetData getData = JsonUtility.FromJson<JS_GetData>(response);
                string s = getData.jsondata;
                // the content of the response from JS has a pair of quotes around it because it is saved as a string in a key/value pair, remove them
                if (s[0] == '\"' && s[s.Length - 1] == '\"')
                    s = s.Substring(1, s.Length - 2);
                // remove escape characters before the quotes embedded in the string
                obj = s.Replace("\\" + "\"", "\"");
                isSuccess = string.IsNullOrEmpty(getData.error);
                break;

            case "custom_score":
                JS_ScoreTable getTable = JsonUtility.FromJson<JS_ScoreTable>(response);
                obj = getTable;
                isSuccess = getTable.errorcode == 0;
                break;

            case "tables":
                JS_ScoreTables getTables = JsonUtility.FromJson<JS_ScoreTables>(response);
                obj = getTables;
                isSuccess = getTables.errorcode == 0;
                break;

            case "show_ad":
            case "app_request":
                break;

            case "blacklist":
            case "sponsor":
                bool.TryParse(response, out bool isTrue);
                obj = isTrue;
                break;

            default:
                Debug.Log($"Unhandled request type: {request}");
                break;
        }

        // callback when all is completed
        if (invokedCalls.TryGetValue(_id, out Action<int, object, bool> callback))
        {
            callback(_id, obj, isSuccess);
            invokedCalls.Remove(_id);
        }
        else
        {
            Debug.Log($"There is no callback for response to id {_id} ");
        }
    }
}
