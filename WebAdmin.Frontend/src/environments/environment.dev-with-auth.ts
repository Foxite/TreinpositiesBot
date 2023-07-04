import {AuthConfig} from "angular-oauth2-oidc";

export const environment = {
  useAuth: true,
  apiUrl: "http://localhost:8180",
};

export const authCodeFlowConfig: AuthConfig = {
  tokenEndpoint: "https://discord.com/oauth2/authorize",
  userinfoEndpoint: "https://discord.com/api/oauth2/token",

  issuer: 'https://id.corsac.nl/',

  // URL of the SPA to redirect the user to after login
  redirectUri: window.location.origin,

  // The SPA's id. The SPA is registerd with this id at the auth-server
  // clientId: 'server.code',
  clientId: '7e8666ffdbf3c11826940541b02b60e03de51c1a',

  // Just needed if your auth server demands a secret. In general, this
  // is a sign that the auth server is not configured with SPAs in mind
  // and it might not enforce further best practices vital for security
  // such applications.
  // dummyClientSecret: 'secret',

  responseType: 'code',

  // set the scope for the permissions the client should request
  // The first four are defined by OIDC.
  // Important: Request offline_access to get a refresh token
  // The api scope is a usecase specific one
  scope: 'openid profile mellifera.read',

  showDebugInformation: true,
};

