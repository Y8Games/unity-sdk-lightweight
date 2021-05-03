
#if !UNITY_EDITOR

// data structures for the JS responses to be deserialised into

//
// Auth Response - complex structure requires many classes...
//

[System.Serializable]
public class JS_Authorisation
{
    public JS_AuthResponse authResponse = null;
    public string status = "";
}

[System.Serializable]
public class JS_AuthResponse
{
    public string state = null;
    public string access_token = null;
    public string token_type = null;
    public int expires_in = 0;
    public string score = null;
    public string redirect_uri = null;
    public JS_Details details = null;
}

[System.Serializable]
public class JS_Details
{
    public int level = 0;
    public JS_TrustDetails trust_details = null;
    public string first_name = null;
    public string dob = null;
    public string language = null;
    public string gender = null;
    public string nickname = null;
    public string pid = null;
    public string locale = null;
    public JS_Avatars avatars = null;
    public string version = null;
    public JS_Risk risk = null;
}

[System.Serializable]
public class JS_TrustDetails
{
    public string email = null;
    public bool mobile = false;
    public bool certification = false;
}

[System.Serializable]
public class JS_Avatars
{
    public string thumb_url = null;
    public string thumb_secure_url = null;
    public string medium_url = null;
    public string medium_secure_url = null;
    public string large_url = null;
    public string large_secure_url = null;
}

[System.Serializable]
public class JS_Risk
{
    public JS_RiskElement registration = null;
    public JS_RiskElement login = null;
}

[System.Serializable]
public class JS_RiskElement
{
    public string risk = null;
    public string real_ip = null;
    public string request_ip = null;
}


//
// achievement_save response
//

[System.Serializable]
public class JS_AchievementSave
{
    public string name = "";
    public bool unlocked = false;
    public int errorcode = 0;
    public bool success = false;
    public string errormessage = "";
};


//
// score_save response
//

[System.Serializable]
public class JS_ScoreSave
{
    public int errorcode = 0;
    public bool success = false;
    public string errormessage = "";
};


//
// set_data response
//

[System.Serializable]
public class JS_SetData
{
    public string status = "";
    public string key = "";
};


//
// get_data response
//

[System.Serializable]
public class JS_GetData
{
    public string error = "";
    public string key = "";
    public string jsondata = "";
};


//
// custom_score response
//

[System.Serializable]
public class JS_ScoreTable
{
    public JS_Score[] scores;
    public int numscores;
    public string mode;
    public int errorcode;
    public bool success;
};

[System.Serializable]
public class JS_Score
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


//
// tables response
//

[System.Serializable]
public class JS_ScoreTables
{
    public string[] tables;
    public int errorcode;
    public bool success;
};

#endif
