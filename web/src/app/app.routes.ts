import { Router, Routes } from '@angular/router'
import { inject } from '@angular/core'
import { Layout } from './layout/layout/layout'
import { AuthLayout } from './layout/auth-layout/auth-layout'
import { AuthService } from '@/app/services/auth.service'
import { EnvService } from '@/app/services/env.service'

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthLayout,
    loadChildren: () =>
      import('@views/auth/auth.route').then((mod) => mod.AUTH_ROUTES),
    canActivate: [
      () => {
        const authService = inject(AuthService)
        const env = inject(EnvService)

        if (authService.IsLoggedIn()) return true

        const identity = localStorage.getItem('database') ?? ''
        const random = Math.random().toString(36).slice(2)
        const stateParam = identity ? `${random}:${identity}` : random

        const params = new URLSearchParams({
          client_id: env.ssoClientId,
          redirect_uri: env.ssoRedirectUri,
          state: stateParam,
        })
        window.location.href = `${env.ssoApiUrl}/auth/authorize?${params}`
        return false
      },
    ],
  },
  {
    path: 'sso-callback',
    loadComponent: () =>
      import('./views/auth/sso-callback/sso-callback.component').then(
        (m) => m.SsoCallbackComponent
      ),
  },
  {
    path: '',
    component: Layout,
    loadChildren: () =>
      import('./views/views.route').then((mod) => mod.VIEWS_ROUTES),
    canActivate: [
      (_route: any, state: any) => {
        const authService = inject(AuthService)
        const env = inject(EnvService)

        if (!authService.IsLoggedIn()) {
          const identity = localStorage.getItem('database') ?? ''
          const random = Math.random().toString(36).slice(2)
          const stateParam = identity ? `${random}:${identity}` : random

          const params = new URLSearchParams({
            client_id: env.ssoClientId,
            redirect_uri: env.ssoRedirectUri,
            state: stateParam,
          })
          window.location.href = `${env.ssoApiUrl}/auth/authorize?${params}`
          return false
        }

        return true
      },
    ],
  },
  { path: '**', redirectTo: '' },
]
