import { Router, Routes } from '@angular/router'
import { inject } from '@angular/core'
import { HttpBackend, HttpClient } from '@angular/common/http'
import { firstValueFrom } from 'rxjs'
import { Layout } from './layout/layout/layout'
import { AuthLayout } from './layout/auth-layout/auth-layout'
import { AuthService } from '@/app/services/auth.service'
import { EnvService } from '@/app/services/env.service'

function redirectToSso(env: EnvService): false {
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

async function tryRefreshOrRedirect(env: EnvService): Promise<boolean> {
  const refreshToken = localStorage.getItem('refreshToken')
  let database = localStorage.getItem('database')

  if (!database) {
    try {
      const tkn = localStorage.getItem('token')
      if (tkn) {
        const payload = JSON.parse(atob(tkn.split('.')[1]))
        database = payload['Database'] ?? payload['database'] ?? null
        if (database) localStorage.setItem('database', database)
      }
    } catch {}
  }

  if (refreshToken && database) {
    try {
      const backend = inject(HttpBackend)
      const http = new HttpClient(backend)
      const res = await firstValueFrom(
        http.post<{ isSuccess: boolean; data: { token: string; refreshToken: string } }>(
          `${env.apiUrl}/api/auth/refresh`,
          { refreshToken, identity: database }
        )
      )
      if (res.isSuccess && res.data?.token) {
        localStorage.setItem('token', res.data.token)
        if (res.data.refreshToken) localStorage.setItem('refreshToken', res.data.refreshToken)
        return true
      }
    } catch {}
  }

  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('user')
  localStorage.removeItem('database')
  return redirectToSso(env)
}

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
        return redirectToSso(env)
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
      async () => {
        const authService = inject(AuthService)
        const env = inject(EnvService)
        if (authService.IsLoggedIn()) return true
        return tryRefreshOrRedirect(env)
      },
    ],
  },
  { path: '**', redirectTo: '' },
]
