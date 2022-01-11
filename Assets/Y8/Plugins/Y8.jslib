mergeInto(LibraryManager.library,
{
    Init: function( _appId, _adsId )
    {
		var appId = Pointer_stringify( _appId );
		var adsId = Pointer_stringify( _adsId );

        window.idAsyncInit = function()
            {
                // Triggered when the SDK is init
                ID.Event.subscribe('id.init', function()
                    {		
						if (adsId) {
							ID.ads.init(adsId);
						} else {
							console.log("Ads ID is empty");
						}									
						
						ID.GameAPI.init(appId, null, function(data, response)
							{
								//console.log(data, response);
								// tell c# that the system is fully ready
								SendMessage('Y8_Root', 'CallbackReady');
							});
                    });

				// Triggered when a user logs in or register
				ID.Event.subscribe('auth.authResponseChange', function(auth)
					{
						//console.log("authResponseChange event:" + auth);
						// send the login data to c#
						SendMessage('Y8_Root', 'AuthCallbackResponse', JSON.stringify(auth));
					});

                // init the JS interface
                ID.init(
                    {
                        appId : appId,
						responseType: 'token',
                    });
            };

        // load the js sdk
        (function(d, s, id)
            {
                var js, fjs = d.getElementsByTagName(s)[0];
                if (d.getElementById(id)) {return;}
                js = d.createElement(s); js.id = id;
                js.src =  document.location.protocol == 'https:' ? "https://cdn.y8.com/api/sdk.js" : "http://cdn.y8.com/api/sdk.js";
                fjs.parentNode.insertBefore(js, fjs);
            }(document, 'script', 'id-jssdk')
        );
    },

    Call: function( _id, _request, _jsonData )
    {
		// convert this pointer back into the Call request string
		var request = Pointer_stringify( _request );

		var jsonData = null, jsonString = "";
		if (_jsonData)
		{
			jsonString = Pointer_stringify( _jsonData );
			if (jsonString && jsonString.length > 2)
				jsonData = JSON.parse(jsonString);
		}

        // callback to capture response and send it to c#
        var idCallback = function(response)
        {
            if (response != undefined && response != null)
            {	// the server processed the response

				// login request responses are handled by ID.Event.subscribe('auth.authResponseChange' in Init()
				if (request != 'login' && request != 'auto_login')
				{
					var json = JSON.stringify(response);
					//console.log("JS response to [" + _id + "] = " + json);
					SendMessage('Y8_Root', 'CallbackResponse', request + '[' + _id + ']=' + json);
				}
            }
			else
			{
				SendMessage('Y8_Root', 'CallbackResponse', request + '[' + _id + ']=' + '{}');
			}
        }

        // callback when highscore list is closed to let c# know
        var idCallbackList = function()
        {
			SendMessage('Y8_Root', 'CallbackResponse', request + '[' + _id + ']=' + '{}');
			ID.Event.unsubscribe('dialog.hide', idCallbackList);
        }

        // callback to capture data and response and send it to c#
        var idCallbackData = function(data, response)
        {
			if (data)
			{
				//console.log("JS " + data);
				SendMessage('Y8_Root', 'CallbackResponse', request + '[' + _id + ']=' + JSON.stringify(data));
			}
            if (response)
            {
                //console.log("JS " + response);
				SendMessage('Y8_Root', 'CallbackResponse', _id, request + '[' + _id + ']=' + JSON.stringify(response));
            }
        }

		// call the appropriate ID function
        switch( request )
        {
			case 'auto_login':
				ID.getLoginStatus(idCallback.bind(this));
				break;

            case 'login':
				ID.login(idCallback.bind(this));
				break;

			case 'show_ad':
				ID.ads.display(idCallback.bind(this));
				break;

            case 'register':
				ID.register(idCallback.bind(this));
				break;

			case 'achievement_list':
				ID.GameAPI.Achievements.list(null);
				break;
			
			case 'get_achievements':
				ID.GameAPI.Achievements.listCustom({}, idCallbackData.bind(this));
				break;

			case 'achievement_save':
				ID.GameAPI.Achievements.save(jsonData, idCallbackData.bind(this));
				break;
				
			case 'tables':
				ID.GameAPI.Leaderboards.tables(null, idCallbackData.bind(this));
				break;

			case 'custom_score':
				ID.GameAPI.Leaderboards.listCustom(jsonData, idCallbackData.bind(this));
				break;

			case 'score_list':
				ID.Event.subscribe('dialog.hide', idCallbackList.bind(this));
				ID.GameAPI.Leaderboards.list(jsonData);
				break;

			case 'score_save':
				ID.GameAPI.Leaderboards.save(jsonData, idCallbackData.bind(this));
				break;

			case 'app_request':
				ID.ui(jsonData, idCallback.bind(this));
				break;

			case 'friend_request':
				ID.ui(jsonData, idCallback.bind(this));
				break;

			case 'share':
			    ID.ui(jsonData, idCallback.bind(this));
				break;

			case 'set_data':
				ID.api('user_data/submit', 'POST', jsonData, idCallback.bind(this));
				break;

			case 'get_data':
				ID.api('user_data/retrieve', 'POST', jsonData, idCallback.bind(this));
				break;

			case 'clear_data':
				ID.api('user_data/remove', 'POST', jsonData, idCallback.bind(this));
				break;
				
			case 'blacklist':
				ID.Protection.isBlacklisted(idCallback.bind(this));
				break;
				
			case 'sponsor':
				ID.Protection.isSponsor(idCallback.bind(this));
				break;

			case 'save_screenshot':				
				ID.submit_image(jsonData['data'], idCallback.bind(this));
				break;
        }
    }

});
