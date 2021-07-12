#if !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

namespace Y8API
{
    public class Y8 : MonoBehaviour
    {
        public static Y8 Instance { get; private set; }

        private static bool isReady = false;

        [Header("ENTER APP ID HERE")] public string AppId = "";
        [Header("ENTER ADS ID HERE (optional)")] public string AdsId = "";

        private int id = 10000;

        // Jslib invokes methods on object with specific name > we need to enforce it
        private readonly string calleeName = "Y8_Root";

        private readonly Dictionary<int, object> callIdToResponse = new Dictionary<int, object>();

        private Authorisation auth;

        private void Awake()
        {
            if (Instance == null)
            {
                isReady = false;
                Instance = this;
                gameObject.name = calleeName;
                transform.SetParent(null);

                AppId = AppId.Trim();
                AdsId = AdsId.Trim();

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
        public async Task<JsResponse<Authorisation>> AutoLoginAsync()
        {
            return await TryCallAsync<Authorisation>("auto_login", null);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/auth-functions/
        /// Opens a menu showing fields needed to register a new user. If the user is already logged in, it will close the menu and return to the redirect URI or callback.
        /// </summary>
        public async Task<JsResponse<Authorisation>> RegisterAsync()
        {
            return await TryCallAsync<Authorisation>("register", null);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/auth-functions/
        /// It is the same process as Register
        /// You may use the same callback and options for Register and Login
        /// </summary>
        public async Task<JsResponse<Authorisation>> LoginAsync()
        {
            return await TryCallAsync<Authorisation>("login", null);
        }

        public async Task ShowAdAsync()
        {
            if (Screen.fullScreen)
            {
                Debug.Log("Game is running in fullscreen mode, skipping ads");
                return;
            }

            if (string.IsNullOrEmpty(AdsId))
            {
                Debug.Log("Ads ID is not set! Please contact the support to receive it if you want to show ads");
                return;
            }

            await TryCallAsync<Empty>("show_ad", null);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/game-api/
        /// Will display all achievements. If a player is logged in, it will display which achievements have been unlocked.
        /// </summary>
        public async Task ShowAchievementListAsync()
        {
            await TryCallAsync<Empty>("achievement_list", null);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/game-api/
        /// </summary>
        /// <param name="achievement">The title of the achievement. This must exactly match.</param>
        /// <param name="achievementkey">The unlock key generated from the achievements application page.This must also exactly match.</param>
        /// <param name="overwrite">(optional) (default: false) Allow players to unlock the same achievement more than once.</param>
        /// <param name="allowduplicates">(optional)(default: false) Allow players to unlock the same achievement and display them seperatly.</param>
        public async Task<JsResponse<AchievementSave>> SaveAchievementAsync(string achievement, string achievementkey, bool overwrite = false, bool allowduplicates = false)
        {
            if (!IsLoggedIn())
            {
                Debug.Log("Player is not logged in! Can't use AchievementSave");
                return new JsResponse<AchievementSave>(false, default);
            }

            KeyValuePair<string, string>[] json = {
                new KeyValuePair<string, string>("achievement", achievement),
                new KeyValuePair<string, string>("achievementkey", achievementkey),
                new KeyValuePair<string, string>("overwrite", overwrite.ToString().ToLower()),
                new KeyValuePair<string, string>("allowduplicates", allowduplicates.ToString().ToLower())
            };

            return await TryCallAsync<AchievementSave>("achievement_save", json);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/game-api/
        /// Return an app’s tables to a callback. Useful only when table names are unknown.
        /// </summary>
        public async Task<JsResponse<ScoreTables>> GetTableNamesAsync()
        {
            return await TryCallAsync<ScoreTables>("tables", null);
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
        public async Task<JsResponse<ScoreTable>> GetCustomScoreAsync(string table, string mode = "alltime", int perPage = 20, int page = 1, bool highest = true, string playerid = "")
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

            return await TryCallAsync<ScoreTable>("custom_score", json.ToArray());
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/game-api/
        /// Display the high scores menu.
        /// </summary>
        /// <param name="tableTitle">The exact table name from the app’s high scores page at y8.com. This is also the menu title.</param>
        /// <param name="mode">(optional) A string that equals alltime, last30days, last7days, today, or newest.</param>
        /// <param name="highest">(optional)(default: true) Set to false if a lower score is better.</param>
        /// <param name="useMilli">(optional)(default: false) Render scores in milliseconds.</param>
        public async Task ShowScoreListAsync(string tableTitle, string mode = "alltime", bool highest = true, bool useMilli = false)
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

            await TryCallAsync<Empty>("score_list", json.ToArray());
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/game-api/
        /// Saves a player score.
        /// </summary>
        /// <param name="table">The exact table name from the app’s high scores page at Y8.com.</param>
        /// <param name="points">A number representing the player’s score</param>
        /// <param name="allowduplicates">(optional) (default: false) Set to true if player’s can submit more than one score.</param>
        /// <param name="highest">(optional) (default: true) Set to false if a lower score is better.</param>
        public async Task<JsResponse<ScoreSave>> SaveScoreAsync(string table, int points, bool allowduplicates = false, bool highest = true)
        {
            if (!IsLoggedIn())
            {
                Debug.Log("Player is not logged in! Can't use ScoreSave");
                return new JsResponse<ScoreSave>(false, default);
            }

            List<KeyValuePair<string, string>> json = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("table", table),
                new KeyValuePair<string, string>("points", points.ToString()),
                new KeyValuePair<string, string>("allowduplicates", allowduplicates.ToString().ToLower()),
                new KeyValuePair<string, string>("highest", highest.ToString().ToLower()),
                new KeyValuePair<string, string>("playername", Nickname())
            };

            return await TryCallAsync<ScoreSave>("score_save", json.ToArray());
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
        public async Task AppRequestAsync(string message = "<message>", string redirect_uri = "", string data = "")
        {
            KeyValuePair<string, string>[] json = {
                new KeyValuePair<string, string>("method", "apprequests"),
                new KeyValuePair<string, string>("message", message),
                new KeyValuePair<string, string>("redirect_uri", redirect_uri),
                new KeyValuePair<string, string>("data", data)
            };

            await TryCallAsync<Empty>("app_request", json);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/friend-request-dialog/
        /// Y8.com SDK provides the ability to send a friend request from your application
        /// </summary>
        /// <param name="redirect_uri">The URL to redirect to after a person clicks a button on the dialog.</param>
        /// <param name="id">PID of the user in the application.</param>
        public async Task FriendRequestAsync(string _id, string redirect_uri = "")
        {
            KeyValuePair<string, string>[] json = {
                new KeyValuePair<string, string>("method", "friends"),
                new KeyValuePair<string, string>("id", _id),
                new KeyValuePair<string, string>("redirect_uri", redirect_uri)
            };

            await TryCallAsync<Empty>("friend_request", json);
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
        public async Task ShareAsync(string link, string description, string name = "", string caption = "", string picture = "")
        {
            KeyValuePair<string, string>[] json = {
                new KeyValuePair<string, string>("method", "feed"),
                new KeyValuePair<string, string>("link", link),
                new KeyValuePair<string, string>("description", description),
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("caption", caption),
                new KeyValuePair<string, string>("picture", picture),
            };

            await TryCallAsync<Empty>("share", json);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/online-saves/
        /// Save the value, for later retrieval using the key.
        /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
        /// If you want to save frequently, such as when a player changes a setting, the code must buffer and retry failed submits. Submitting data could fail if things are saved too freqently.
        /// </summary>
        /// <param name="key">The key (or name) to be stored.</param>
        /// <param name="value">The value to be stored for access with that key.</param>
        public async Task<JsResponse<SetData>> SetDataAsync(string key, string value)
        {
            if (!IsLoggedIn())
            {
                Debug.Log("Player is not logged in! Can't use SetData");
                return new JsResponse<SetData>(false, default);
            }

            KeyValuePair<string, string>[] json = {
                new KeyValuePair<string, string>("key", key),
                new KeyValuePair<string, string>("value", value)
            };

            return await TryCallAsync<SetData>("set_data", json);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/online-saves/
        /// Retrieve the value saved using this key.
        /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
        /// </summary>
        /// <param name="key">The key (or name) to be retrieved.</param>
        public async Task<JsResponse<GetData>> GetDataAsync(string key)
        {
            if (!IsLoggedIn())
            {
                Debug.Log("Player is not logged in! Can't use GetData");
                return new JsResponse<GetData>(false, default);
            }

            KeyValuePair<string, string>[] json = {
                new KeyValuePair<string, string>("key", key)
            };

            return await TryCallAsync<GetData>("get_data", json);
        }

        /// <summary>
        /// https://docs.y8.com/docs/javascript/online-saves/
        /// Remove a key/value pair from the saved data.
        /// The online save API provides user data storage for applications. It is useful for storing game states and other small data sets that a user may want to reuse later on a different device.
        /// </summary>
        /// <param name="key">The key (or name) to be deleted with its corresponding value.</param>
        public async Task<JsResponse<SetData>> ClearDataAsync(string key)
        {
            if (!IsLoggedIn())
            {
                Debug.Log("Player is not logged in! Can't use ClearData");
                return new JsResponse<SetData>(false, default);
            }

            KeyValuePair<string, string>[] json = {
                new KeyValuePair<string, string>("key", key)
            };

            return await TryCallAsync<SetData>("clear_data", json);
        }

        /// <summary>
        /// Check if the current URL is blacklisted
        /// </summary>
        /// <returns>true if it is</returns>
        public async Task<JsResponse<bool>> IsBlacklistedAsync()
        {
            return await TryCallAsync<bool>("blacklist", null);
        }

        /// <summary>
        /// Check if the current URL is a sponsor
        /// </summary>
        /// <returns>true if it is</returns>
        public async Task<JsResponse<bool>> IsSponsorAsync()
        {
            return await TryCallAsync<bool>("sponsor", null);
        }

        ///
        /// Quick access methods to immediately return values (often acquired in the user authentication response)
        ///

        /// <summary>
        /// true if the user is logged in to Y8.com
        /// </summary>
        public bool IsLoggedIn()
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

        private async Task<JsResponse<T>> TryCallAsync<T>(string requestName, KeyValuePair<string, string>[] kvPairs)
        {
            if (!isReady)
            {
                Debug.Log("SDK is not ready");
                return new JsResponse<T>(false, default);
            }

            id++;
            int callId = id;

            string json = ConvertListToJson(kvPairs);
            Debug.Log($"JS call [{callId}] with JSON = {json}");
            Call(id, requestName, json);

            while (!callIdToResponse.ContainsKey(callId)) await Task.Yield();

            object response = callIdToResponse[callId];
            callIdToResponse.Remove(callId);

            return (JsResponse<T>)response;
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
            Debug.Log($"AuthResponse from JS: {_response} ");
            auth = JsonUtility.FromJson<Authorisation>(_response);

            callIdToResponse.Add(authId, new JsResponse<Authorisation>(IsLoggedIn(), auth));
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
            string responseData = _responseString.Substring(cb + 2);
            Debug.Log($"Response from JS: {request}[{_id}] = '{responseData}'");

            object response = responseData;      // default value to the response string

            // parse the response JSON into the correct C# class object so it can be accessed easily
            switch (request)
            {
                case "achievement_save":
                    AchievementSave achSave = JsonUtility.FromJson<AchievementSave>(responseData);
                    response = new JsResponse<AchievementSave>(achSave.success, achSave);
                    break;

                case "score_save":
                    ScoreSave scoreSave = JsonUtility.FromJson<ScoreSave>(responseData);
                    response = new JsResponse<ScoreSave>(scoreSave.success, scoreSave);
                    break;

                case "set_data":
                case "clear_data":
                    SetData setData = JsonUtility.FromJson<SetData>(responseData);
                    response = new JsResponse<SetData>(setData.status == "ok", setData);
                    break;

                case "get_data":
                    GetData getData = JsonUtility.FromJson<GetData>(responseData);
                    response = new JsResponse<GetData>(string.IsNullOrEmpty(getData.error), getData);
                    break;

                case "custom_score":
                    ScoreTable getTable = JsonUtility.FromJson<ScoreTable>(responseData);
                    response = new JsResponse<ScoreTable>(getTable.errorcode == 0, getTable);
                    break;

                case "tables":
                    ScoreTables getTables = JsonUtility.FromJson<ScoreTables>(responseData);
                    response = new JsResponse<ScoreTables>(getTables.errorcode == 0, getTables);
                    break;

                case "show_ad":
                case "share":
                case "score_list":
                case "app_request":
                case "friend_request":
                case "achievement_list":
                    response = new JsResponse<Empty>(true, default);
                    break;

                case "blacklist":
                case "sponsor":
                    bool.TryParse(responseData, out bool isTrue);
                    response = new JsResponse<bool>(true, isTrue);
                    break;

                default:
                    Debug.Log($"Unhandled request type: {request}");
                    break;
            }

            callIdToResponse.Add(_id, response);
        }
    }
}
