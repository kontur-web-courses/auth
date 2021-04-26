export const ApplicationName = "Photos SPA";

export const QueryParameterNames = {
  ReturnUrl: "returnUrl",
  Message: "message",
};

export const LoginActions = {
  Login: "login",
  LoginCallback: "signin-passport",
  LoginFailed: "login-failed",
  Profile: "profile",
  Register: "register",
};

export const LogoutActions = {
  Logout: "logout",
  LogoutCallback: "signout-passport",
  LoggedOut: "logged-out",
};

const prefix = "/authentication";

export const ApplicationPaths = {
  ApiAuthorizationPrefix: prefix,

  Login: `${prefix}/${LoginActions.Login}`,
  LoginCallback: `${prefix}/${LoginActions.LoginCallback}`,
  LoginFailed: `${prefix}/${LoginActions.LoginFailed}`,
  Register: `${prefix}/${LoginActions.Register}`,
  Profile: `${prefix}/${LoginActions.Profile}`,
  LogOut: `${prefix}/${LogoutActions.Logout}`,
  LogOutCallback: `${prefix}/${LogoutActions.LogoutCallback}`,
  LoggedOut: `${prefix}/${LogoutActions.LoggedOut}`,
  RegisterRedirectUrl: "https://localhost:7001",
  ProfileRedirectUrl: "https://localhost:7001/diagnostics",
};

export const ApiAuthorizationClientConfiguration = {
  // обязательные настройки
  authority: "https://localhost:7001",
  client_id: "Photos SPA",
  redirect_uri: "https://localhost:8001/authentication/signin-passport",
  post_logout_redirect_uri:
    "https://localhost:8001/authentication/signout-passport",
  response_type: "code",
  scope: "openid profile email photos",
  // опциональные настройки
  // https://github.com/IdentityModel/oidc-client-js/wiki#other-optional-settings
};
