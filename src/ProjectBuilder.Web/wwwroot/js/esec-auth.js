
(function () {
    function storeTokensFromServer(tokenData) {
        if (!tokenData) return;

        localStorage.setItem("access_token", tokenData.access_token);
        localStorage.setItem("refresh_token", tokenData.refresh_token);
        localStorage.setItem("token_id", tokenData.token_id);
        localStorage.setItem("TokenExpiration", tokenData.expiration);
        localStorage.setItem("UserTokens", JSON.stringify({
            access_token: tokenData.access_token,
            refresh_token: tokenData.refresh_token,
            id_token: tokenData.token_id
        }));
    }

    function getRemainingAccessTokenTime() {
        const expiration = parseInt(localStorage.getItem('TokenExpiration'));
        const now = Date.now();
        return (expiration - now) / 60000;
    }

    function refreshAccessTokenIfCloseToExpiration() {
        const timeLeft = getRemainingAccessTokenTime();
        if (timeLeft > 10) return;

        const userTokensRaw = localStorage.getItem("UserTokens");
        if (!userTokensRaw) return;

        const userTokens = JSON.parse(userTokensRaw);
        const refreshToken = userTokens.refresh_token;
        if (!refreshToken) return;
        const token = $('meta[name="xsrf-token"]').attr('content');

        $.ajax({
            url: '/ESECAccount/GetRefreshToken',
            method: 'POST',
            data: { refreshToken },
            headers: { "RequestVerificationToken": token },
            dataType: 'json',
            success: function (response) {
                if (!response || !response.access_token) return;

                localStorage.setItem("UserTokens", JSON.stringify(response));
                localStorage.setItem("access_token", response.access_token);
                localStorage.setItem("TokenExpiration", (Date.now() + (30 * 60 * 1000)).toString());
            },


            error: function (xhr, status, error) {
                console.error("❌ Token refresh error:", error);
            }
        });
    }

    function logoutEsecUser(event) {
        event.preventDefault();
        document.getElementById('loader-container').style.display = 'block';

        const userTokensRaw = localStorage.getItem("UserTokens");
        if (!userTokensRaw) {
            document.getElementById('loader-container').style.display = 'none';
            return;
        }

        const userTokens = JSON.parse(userTokensRaw);
        const { access_token, refresh_token, id_token } = userTokens;

        $.ajax({
            url: '/ESECAccount/RevokeIdToken',
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + id_token
            },
            complete: function () {
                $.ajax({
                    url: '/ESECAccount/RevokeAccessToken',
                    method: 'POST',
                    data: { token: access_token },
                    complete: function () {
                        $.ajax({
                            url: '/ESECAccount/RevokeRefreshToken',
                            method: 'POST',
                            data: { token: refresh_token },
                            complete: function () {
                                $.ajax({
                                    url: '/ESECAccount/LogoutEsecUser',
                                    method: 'POST',
                                    success: function () {
                                        // Fixed same-origin hop; the server redirects on to the configured ESEC logout page
                                        window.location.href = "/ESECAccount/EsecLogoutRedirect";
                                    },
                                    error: function () {
                                        console.error("LogoutEsecUser action failed.");
                                        window.location.href = "/Home/Index?login=true";
                                    },
                                    complete: function () {
                                        // 🔥 NOW clear localStorage here
                                        localStorage.clear();
                                        document.getElementById('loader-container').style.display = 'none';
                                    }
                                });
                            }
                        });
                    }
                });
            }
        });
    }


    function init(loginFlag, tokenData) {
        if (!loginFlag && tokenData && tokenData.access_token) {
            storeTokensFromServer(tokenData);
        }

        setInterval(refreshAccessTokenIfCloseToExpiration, 3 * 60 * 1000);
        refreshAccessTokenIfCloseToExpiration();

        $('.esec-logout-btn').on('click', logoutEsecUser);

        if (loginFlag === true && document.getElementById("EsecLogin")) {
            document.getElementById("EsecLogin").href = "/ESECAccount/SignIn?login=true";
        }
    }


    // Expose init globally so view can call it with dynamic values
    window.EsecAuth = {
        init: init
    };
})();
