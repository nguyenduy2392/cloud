import type { Route } from '@angular/router'
import { DevicePairOutlet } from '@/app/views/device-pair-outlet/device-pair-outlet'
import { MobileAuthLogin } from '@/app/views/mobile/m-auth/mobile-auth-login.component'
import { AppLogin } from './app-login/app-login'
import { AppLogout } from './app-logout/app-logout'

export const AUTH_ROUTES: Route[] = [
  {
    path: 'login',
    component: DevicePairOutlet,
    data: {
      title: 'Login',
      mobileCmp: MobileAuthLogin,
      desktopCmp: AppLogin,
    },
  },
  {
    path: 'logout',
    component: AppLogout,
    data: { title: 'Logout' },
  }
]

