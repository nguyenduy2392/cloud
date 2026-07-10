import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpClient,
  HttpBackend,
} from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable, Subject, throwError } from 'rxjs'
import { catchError } from 'rxjs/operators'
import { EnvService } from '@/app/services/env.service'

let isRefreshing = false
let refreshSubject: Subject<string | null> | null = null

function logout(): void {
  isRefreshing = false
  refreshSubject = null
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('user')
  localStorage.removeItem('database')
  window.location.href = '/auth/login'
}

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private http: HttpClient

  constructor(backend: HttpBackend, private env: EnvService) {
    // Dùng HttpBackend để bypass interceptor chain, tránh vòng lặp
    this.http = new HttpClient(backend)
  }

  intercept(
    request: HttpRequest<Request>,
    next: HttpHandler
  ): Observable<HttpEvent<Event>> {
    return next.handle(request).pipe(
      catchError((err) => {
        if (err.status === 401 && !request.url.includes('/api/auth/refresh')) {
          return this.handleRefresh(request, next)
        }
        if (err.status === 401) {
          logout()
        }
        const error = err.error?.message || err.statusText
        return throwError(() => error)
      })
    ) as Observable<HttpEvent<Event>>
  }

  private handleRefresh(
    request: HttpRequest<Request>,
    next: HttpHandler
  ): Observable<HttpEvent<Event>> {
    isRefreshing = true

    if (!refreshSubject) {
      refreshSubject = new Subject<string | null>()

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

      const refreshToken = localStorage.getItem('refreshToken')
      if (!refreshToken || !database) {
        logout()
        refreshSubject.next(null)
        refreshSubject.complete()
        refreshSubject = null
        return throwError(() => new Error('No refresh token'))
      }

      this.http
        .post<{ isSuccess: boolean; data: { token: string; refreshToken: string } }>(
          `${this.env.apiUrl}/api/auth/refresh`,
          { refreshToken, identity: database }
        )
        .subscribe({
          next: (res) => {
            if (!res.isSuccess || !res.data?.token) {
              logout()
              refreshSubject!.next(null)
            } else {
              localStorage.setItem('token', res.data.token)
              if (res.data.refreshToken) localStorage.setItem('refreshToken', res.data.refreshToken)
              isRefreshing = false
              refreshSubject!.next(res.data.token)
            }
            refreshSubject!.complete()
            refreshSubject = null
          },
          error: () => {
            logout()
            refreshSubject!.next(null)
            refreshSubject!.complete()
            refreshSubject = null
          },
        })
    }

    return new Observable<HttpEvent<Event>>((observer) => {
      refreshSubject!.subscribe({
        next: (token) => {
          if (!token) {
            observer.error(new Error('Refresh failed'))
            return
          }
          const retried = request.clone({
            setHeaders: { Authorization: `Bearer ${token}` },
          })
          next.handle(retried as HttpRequest<Request>).subscribe({
            next: (v) => observer.next(v as HttpEvent<Event>),
            error: (e) => observer.error(e),
            complete: () => observer.complete(),
          })
        },
        error: (e) => observer.error(e),
      })
    })
  }
}
