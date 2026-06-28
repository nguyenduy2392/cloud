(function (window) {
  window.__env = window.__env || {};

  window.__env.tenantMap = {
    'localhost': {
      baseApiUrl: 'http://localhost:5124',
      ssoUrl: 'http://localhost:4201',
      ssoApiUrl: 'http://localhost:5265',
      ssoClientId: 'cloud',
      ssoRedirectUri: 'http://localhost:4202/sso-callback'
    },
    'dev-cloud.happyecotech.com': {
      baseApiUrl: '',
      ssoUrl: 'https://dev-sso.happyecotech.com',
      ssoApiUrl: 'https://dev-sso.happyecotech.com',
      ssoClientId: 'cloud',
      ssoRedirectUri: 'https://dev-cloud.happyecotech.com/sso-callback'
    },
    'cloud.happyecotech.com': {
      baseApiUrl: '',
      ssoUrl: 'https://sso.happyecotech.com',
      ssoApiUrl: 'https://sso.happyecotech.com',
      ssoClientId: 'cloud',
      ssoRedirectUri: 'https://cloud.happyecotech.com/sso-callback'
    }
  };
})(this);
