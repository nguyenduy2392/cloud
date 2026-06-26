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
    'minicrm.happyecotech.com': {
      baseApiUrl: '',
      ssoUrl: 'https://sso.happyecotech.com',
      ssoApiUrl: 'https://sso-api.happyecotech.com',
      ssoClientId: 'cloud',
      ssoRedirectUri: 'https://minicrm.happyecotech.com/sso-callback'
    }
  };
})(this);
