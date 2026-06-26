import { Injectable } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { map } from 'rxjs/operators'

import type { Observable } from 'rxjs'
import type { User } from '@core/helper/fake-backend'

/** Cùng khóa với `@/app/services/auth.service` và `MasterService` — chỉ dùng localStorage */
const TOKEN_STORAGE_KEY = 'token'

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
  user: User | null = null

  /** @deprecated Dùng `TOKEN_STORAGE_KEY` / `token` trong localStorage */
  public readonly authSessionKey = TOKEN_STORAGE_KEY

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<User> {
    return this.http.post<User>(`/api/login`, { email, password }).pipe(
      map((user) => {
        if (user && user.token) {
          this.user = user
          this.saveSession(user.token)
        }
        return user
      })
    )
  }

  logout(): void {
    this.removeSession()
    this.user = null
  }

  get session(): string {
    return localStorage.getItem(TOKEN_STORAGE_KEY)?.trim() ?? ''
  }

  saveSession(token: string): void {
    localStorage.setItem(TOKEN_STORAGE_KEY, token)
  }

  removeSession(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY)
  }
}
