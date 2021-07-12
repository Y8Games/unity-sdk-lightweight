using System;

namespace Y8API
{
    [Serializable]
    public class Authorisation
    {
        public AuthResponse authResponse = null;
        public string status = "";
    }

    [Serializable]
    public class AuthResponse
    {
        public string state = null;
        public string access_token = null;
        public string token_type = null;
        public int expires_in = 0;
        public string score = null;
        public string redirect_uri = null;
        public Details details = null;
    }

    [Serializable]
    public class Details
    {
        public int level = 0;
        public TrustDetails trust_details = null;
        public string first_name = null;
        public string dob = null;
        public string language = null;
        public string gender = null;
        public string nickname = null;
        public string pid = null;
        public string locale = null;
        public Avatars avatars = null;
        public string version = null;
        public Risk risk = null;
    }

    [Serializable]
    public class TrustDetails
    {
        public string email = null;
        public bool mobile = false;
        public bool certification = false;
    }

    [Serializable]
    public class Avatars
    {
        public string thumb_url = null;
        public string thumb_secure_url = null;
        public string medium_url = null;
        public string medium_secure_url = null;
        public string large_url = null;
        public string large_secure_url = null;
    }

    [Serializable]
    public class Risk
    {
        public RiskElement registration = null;
        public RiskElement login = null;
    }

    [Serializable]
    public class RiskElement
    {
        public string risk = null;
        public string real_ip = null;
        public string request_ip = null;
    }

    [Serializable]
    public class AchievementSave
    {
        public string name = "";
        public bool unlocked = false;
        public int errorcode = 0;
        public bool success = false;
        public string errormessage = "";
    };

    [Serializable]
    public class ScoreSave
    {
        public int errorcode = 0;
        public bool success = false;
        public string errormessage = "";
    };

    [Serializable]
    public class SetData
    {
        public string status = "";
        public string key = "";
    };

    [Serializable]
    public class GetData
    {
        public string error = "";
        public string key = "";
        public string jsondata = "";
    };

    [Serializable]
    public class ScoreTable
    {
        public Score[] scores;
        public int numscores;
        public string mode;
        public int errorcode;
        public bool success;
    };

    [Serializable]
    public class Score
    {
        public string table;
        public string playerid;
        public string playername;
        public string appid;
        public string tableid;
        public int points;
        public object fields;
        public int lastupdated;
        public int date;
        public int rank;
        public string scoreid;
        public string rdate;
    };

    [Serializable]
    public class ScoreTables
    {
        public string[] tables;
        public int errorcode;
        public bool success;
    };

    [Serializable]
    public class JsResponse<T>
    {
        public bool IsSuccess { get; }
        public T Data { get; }

        public JsResponse(bool isSuccess, T data)
        {
            IsSuccess = isSuccess;
            Data = data;
        }
    }

    public class Empty
    {
    }
}
