export const ApplicationName = "Photos SPA";

export const QueryParameterNames = {
  ReturnUrl: "returnUrl",
  Message: "message",
};

export const LoginActions = {
  Login: "login",
  LoginCallback: "login-callback",
  LoginFailed: "login-failed",
  Profile: "profile",
  Register: "register",
};

export const LogoutActions = {
  Logout: "logout",
  LogoutCallback: "logout-callback",
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
  redirect_uri: `https://localhost:8001${ApplicationPaths.LoginCallback}`,
  post_logout_redirect_uri: `https://localhost:8001${ApplicationPaths.LogOutCallback}`,
  response_type: "code",
  scope: "photos profile email openid",
  automaticSilentRenew: true, // ÐÑÑÐ°ÑÑÑÑ Ð»Ð¸ Ð°Ð²ÑÐ¾Ð¼Ð°ÑÐ¸ÑÐµÑÐºÐ¸ Ð¾Ð±Ð½Ð¾Ð²Ð»ÑÑÑ access token Ð¿ÐµÑÐµÐ´ Ð¸ÑÑÐµÑÐµÐ½Ð¸ÐµÐ¼ ÑÑÐ¾ÐºÐ° ÐµÐ³Ð¾ Ð´ÐµÐ¹ÑÑÐ²Ð¸Ñ
  accessTokenExpiringNotificationTime: 30, // ÐÐ° ÑÐºÐ¾Ð»ÑÐºÐ¾ ÑÐµÐºÑÐ½Ð´ Ð´Ð¾ Ð¸ÑÑÐµÑÐµÐ½Ð¸Ñ ÑÑÐ¾ÐºÐ° Ð´ÐµÐ¹ÑÑÐ²Ð¸Ñ access token Ð´ÐµÐ»Ð°ÑÑ Ð¿Ð¾Ð¿ÑÑÐºÑ ÐµÐ³Ð¾ Ð¾Ð±Ð½Ð¾Ð²Ð»ÐµÐ½Ð¸Ñ
  includeIdTokenInSilentRenew: true, // ÐÐºÐ»ÑÑÐ°ÑÑ Ð»Ð¸ id_token Ð² ÐºÐ°ÑÐµÑÑÐ²Ðµ id_token_hint Ð¿ÑÐ¸ Ð²ÑÐ·Ð¾Ð²Ð°Ñ silent renew

  // опциональные настройки
  // https://github.com/IdentityModel/oidc-client-js/wiki#other-optional-settings
};


