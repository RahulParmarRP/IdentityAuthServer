import React from "react";
import { UserManager } from 'oidc-client';
import axios from 'axios';

export function getClientSettings() {
    return {
        authority: 'http://localhost:5000',
        client_id: 'angular_spa',
        redirect_uri: 'http://localhost:4200/auth-callback',
        post_logout_redirect_uri: 'http://localhost:4200/',
        response_type: "id_token token",
        scope: "openid profile email api.read",
        filterProtocolClaims: true,
        loadUserInfo: true,
        automaticSilentRenew: true,
        silent_redirect_uri: 'http://localhost:4200/silent-refresh.html'
    };
}

var config = {
    authority: "https://localhost:5001",
    client_id: "spa_example",
    redirect_uri: "https://localhost:5003/callback.html",
    response_type: "code",
    scope: "openid profile api1",
    post_logout_redirect_uri: "https://localhost:5003/index.html",
};

var mgr = new UserManager(config);

function Login() {

    const handleGoogleLogin = () => {
        document.location.href = `https://localhost:5001/api/externalauth2/ExternalLogin?provider=${"google"}&returnUrl=${window.location.href}`;

        // axios.post('https://localhost:5001/api/externalauth2/ExternalLogin',
        //     {
        //         // this should be in responser to resolve CORS
        //         headers: {
        //             // "Access-Control-Allow-Origin": "*",
        //             "Accept": "text/html"
        //             // "Content-Type": "text/html; charset=UTF-8"
        //         },
        //         // params: {
        //         // provider: "google",a
        //         // returnUrl: "http://localhost:3000/index.html"
        //         // }
        //     })
        //     .then((response) => {
        //         debugger;
        //     });
        // .catch((res) => {
        //     debugger;
        // });
        /*
        As we stated earlier, this end point will accept the GET 
        requests originated from our AngularJS app, 
        so it will accept GET request on the form:
         http://ngauthenticationapi.azurewebsites.net/api/Account/ExternalLogin?
         provider=Google&response_type=token&client_id=ngAuthApp&
         redirect_uri=http://ngauthenticationweb.azurewebsites.net/authcomplete.html
    
         /api/Account/ExternalLogin?
         provider=Google&
         response_type=token&
         client_id=self&
         redirect_uri=https://localhost/&
         state=as..aaa
         */
    };

    return (
        <div>
            <button onClick={handleGoogleLogin}> Sign in with Google </button>
        </div>
    );
}

export default Login;
