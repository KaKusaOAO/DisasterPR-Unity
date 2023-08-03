var DiscordIntegrateHelper = {
    $dcState: {
        lastToken: null,
        updateInterval: -1,
        callback: null,
        getAccessToken: function () {
            if (typeof document === "undefined") return "";
            var result = document.cookie.split("access_token=")[1];
            if (result == null) return "";
            result = result.split(";")[0];
            if (result == null) return "";
            return result;
        }
    },
    
    DCSetOnAccessTokenUpdated: function (callback) {
        clearInterval(dcState.updateInterval);
        dcState.callback = callback;
        dcState.updateInterval = setInterval(() => {
            var token = dcState.getAccessToken();
            if (dcState.lastToken != null && token != dcState.lastToken) {
                Module['dynCall_v'](dcState.callback);
            }
            dcState.lastToken = token;
        }, 500);
    },

    DCGetAccessToken: function () {
        var token = dcState.getAccessToken();
        var size = lengthBytesUTF8(token) + 1;
        var buffer = _malloc(size);
        stringToUTF8(token, buffer, size);
        return buffer;
    },
    
    DCStartAuthenticate: function() {
        return !!window.open("http://play.kakaouo.com/u_game/discord/oauth2");
    },

    DCStartAuthenticateNoPopup: function() {
        location.href = "http://play.kakaouo.com/u_game/discord/oauth2?nopopup"
    }
};

autoAddDeps(DiscordIntegrateHelper, '$dcState');
mergeInto(LibraryManager.library, DiscordIntegrateHelper);