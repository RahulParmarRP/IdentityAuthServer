Resource Owner Password Credentials (ROPC) Grant Type
At some point, you’ll be asked why the login page cannot be hosted within the client application, or maybe you’ll have someone on a UX team scream at you for asking them to use a web browser to authenticate the user. Sure, this could be achieved using the ROPC/password grant type; however, this is a security anti-pattern, and this grant type is only included in the OAuth 2.0 specification to help legacy applications. That’s applications considered legacy in 2012.

https://accounts.google.com/o/oauth2/v2/auth/oauthchooseaccount
?response_type=code
&client_id=563770001825-54sjoif5q2i2rqhnt69d7hhjc2khplec.apps.googleusercontent.com
&redirect_uri=https://localhost:5001/signin-google
&scope=openid profile email
&state=CfDJ8MwhuoRtKTVCr3hPlhJz7z4lnnF1X3lgLFElC0AqFh9Q9t4i0BY2eYi3rvhCF9N0z1hl0ROjB3x7na-FxNaaEXinrg3jBGvr9UDF0xy_6zNEgTaB87S7mAFDPbc6o0M2cCZur68rYUAAZfzAE9cuQTjGjBJPtYTW8oa_7UPq49HNY5frf9RA-bHecq1gg1RvPgkMKQc1CCzf2NUr-m-5T_c2Uwx-7KCtnYCr8KLk_lmkop2jPct633nAPOQtWs6mOw
&flowName=GeneralOAuthFlow

https://accounts.google.com/o/oauth2/v2/auth/identifier
?response_type=code
&client_id=563770001825-54sjoif5q2i2rqhnt69d7hhjc2khplec.apps.googleusercontent.com
&redirect_uri=https://localhost:5001/signin-google
&scope=openid profile email
&state=CfDJ8MwhuoRtKTVCr3hPlhJz7z4PfTVnyC4WbKZvhqO9X-4wD8ocpU_zHtH80Wwk2yvQ9f0URnUNeMIJ_PgACR6zUyLXrb6YYuYXuM0fvLm9-HL1sG1S_ePwhBwOquZEMV-mmWtz9XWPt3nm_5ulJ9LXGfB1mwU1MhzHRa3a8U8MrnXE_ZjZ4jhUQpE6lTaoWY1EvF15AMpaeLuWMat3UNS4y7YTFC6fE7mXSTD32eI9Cir4dS0oKghkzvZbY9Bg18ORwvHRq1D-lTRUrGhPfXLuJB_wcuWZtrVTHAPnZxrbpD2B2QJYxkAa1yVIW4Tl7z5YwWtdZWGEQVHeL5xz1uNeK3M&flowName=GeneralOAuthFlow

This project was bootstrapped with [Create React App](https://github.com/facebook/create-react-app).

## Available Scripts

In the project directory, you can run:

### `yarn start`

Runs the app in the development mode.<br />
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.<br />
You will also see any lint errors in the console.

### `yarn test`

Launches the test runner in the interactive watch mode.<br />
See the section about [running tests](https://facebook.github.io/create-react-app/docs/running-tests) for more information.

### `yarn build`

Builds the app for production to the `build` folder.<br />
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.<br />
Your app is ready to be deployed!

See the section about [deployment](https://facebook.github.io/create-react-app/docs/deployment) for more information.

### `yarn eject`

**Note: this is a one-way operation. Once you `eject`, you can’t go back!**

If you aren’t satisfied with the build tool and configuration choices, you can `eject` at any time. This command will remove the single build dependency from your project.

Instead, it will copy all the configuration files and the transitive dependencies (webpack, Babel, ESLint, etc) right into your project so you have full control over them. All of the commands except `eject` will still work, but they will point to the copied scripts so you can tweak them. At this point you’re on your own.

You don’t have to ever use `eject`. The curated feature set is suitable for small and middle deployments, and you shouldn’t feel obligated to use this feature. However we understand that this tool wouldn’t be useful if you couldn’t customize it when you are ready for it.

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).

### Code Splitting

This section has moved here: https://facebook.github.io/create-react-app/docs/code-splitting

### Analyzing the Bundle Size

This section has moved here: https://facebook.github.io/create-react-app/docs/analyzing-the-bundle-size

### Making a Progressive Web App

This section has moved here: https://facebook.github.io/create-react-app/docs/making-a-progressive-web-app

### Advanced Configuration

This section has moved here: https://facebook.github.io/create-react-app/docs/advanced-configuration

### Deployment

This section has moved here: https://facebook.github.io/create-react-app/docs/deployment

### `yarn build` fails to minify

This section has moved here: https://facebook.github.io/create-react-app/docs/troubleshooting#npm-run-build-fails-to-minify
