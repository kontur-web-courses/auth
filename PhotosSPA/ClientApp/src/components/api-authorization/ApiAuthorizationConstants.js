export const ApplicationName = "Photos SPA";

export const QueryParameterNames = {
  ReturnUrl: "returnUrl",
  Message: "message",
};

export const LoginActions = {
  Login: "login",
  LoginCallback: "TODO: путь куда возвращать после логина на сервере авторизации",
  LoginFailed: "login-failed",
  Profile: "profile",
  Register: "register",
};

export const LogoutActions = {
  Logout: "logout",
  LogoutCallback: "TODO: путь куда возвращать после выхода на сервере авторизации",
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
  RegisterRedirectUrl: "TODO: полный url куда отправлять для регистрации",
  ProfileRedirectUrl: "TODO: полный url куда отправлять для просмотра и редактирования профиля",
};

export const ApiAuthorizationClientConfiguration = {
  // обязательные настройки
  authority: "TODO: адрес сервера авторизации",
  client_id: "TODO: идентификатор клиента",
  redirect_uri: "TODO: полный URL куда возвращать после логина на сервере авторизации",
  post_logout_redirect_uri: "TODO: полный URL куда возвращать после выхода на сервере авторизации",
  response_type: "TODO: желаемый тип ответа от сервера авторизации",
  scope: "TODO: запросить все доступные скоупы",
  // опциональные настройки
  // https://github.com/IdentityModel/oidc-client-js/wiki#other-optional-settings
};
