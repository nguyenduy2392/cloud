import { Injectable } from '@angular/core'
import { MasterService } from './master.service'
import { EnvService } from './env.service'
import { Observable, of, switchMap, tap, throwError } from 'rxjs'

/** Khớp với `Core.Common.Response` từ API */
export interface ApiResponse<T = unknown> {
  isSuccess: boolean
  errorCode: number
  message: string
  data: T
}

/** Body khớp `Application.Auth.Dtos.LoginRequest` */
export interface LoginRequest {
  identity: string
  userName: string
  password: string
}

/**
 * User sau đăng nhập — lưu `localStorage` key `user` (JSON, không có `password`).
 * Khớp payload API login / seed (có thể có thêm field theo backend).
 */
export interface AuthUser {
  id: string
  userName: string
  email: string
  avatar: string
  birthday: string | null
  gender: number
  name: string
  address: string
  phone: string
  description: string
  lastLogin: string | null
  isRootAdmin: boolean
  /** Có trên một số bản ghi API */
  isEmployee?: boolean
  appRoleUsers?: unknown | null
  createdAt: string
  createdBy: string | null
  modifiedAt: string
  modifiedBy: string | null
  isDeleted: boolean
  password?: string
}

export interface LoginResponseData {
  token?: string
  user?: AuthUser
  database?: string
}

const USER_STORAGE_KEY = 'user'

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  public get apiURL(): string {
    // Không gán ở field initializer để tránh "used before initialization"
    return this.env.apiUrl
  }
  decodedToken: any
  currentUser: any

  constructor(
    private master: MasterService,
    private env: EnvService
  ) {}

  getTesst() {
    return this.master.get(`${this.apiURL}/WeatherForecast`)
  }

  /// Login — `POST api/auth/login`, response bọc trong `ApiResponse`, JWT nằm ở `data.token`
  login(data: LoginRequest): Observable<ApiResponse<LoginResponseData>> {
    return this.master
      .post<ApiResponse<LoginResponseData>>(
        `${this.apiURL}/api/auth/login`,
        data
      )
      .pipe(
        switchMap((res) => {
          if (!res?.isSuccess) {
            return throwError(() => ({
              error: {
                message: res?.message ?? 'Đăng nhập thất bại.',
              },
            }))
          }
          return of(res)
        }),
        tap((res) => {
          const token = res?.data?.token
          if (typeof token === 'string' && token.length > 0) {
            localStorage.setItem('token', token)
          }
          const user = res?.data?.user
          if (user && typeof user === 'object') {
            const { password: _omit, ...safeUser } = user
            localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(safeUser))
            this.currentUser = safeUser
          }
          const database = res?.data?.database
          if (typeof database === 'string' && database.length > 0) {
            localStorage.setItem('database', database)
          }
        })
      )
  }

  /**
   * Đọc user từ `localStorage` key `user` (chuỗi JSON).
   * `null` nếu chưa có hoặc parse lỗi.
   */
  getStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_STORAGE_KEY)
    if (!raw) return null
    try {
      return JSON.parse(raw) as AuthUser
    } catch {
      return null
    }
  }

  /** Xóa token và phiên đăng nhập (dùng cho nút đăng xuất). */
  logout(): void {
    localStorage.removeItem('token')
    localStorage.removeItem(USER_STORAGE_KEY)
    localStorage.removeItem('database')
    this.currentUser = null
    this.decodedToken = null
  }

  IsLoggedIn() {
    const token = localStorage.getItem('token')
    return token ? !this.isTokenExpired(token) : false
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.decodeJwtPayload(token)
      const exp: number | undefined = payload?.exp
      if (!exp) return true
      return Date.now() / 1000 >= exp
    } catch {
      return true
    }
  }

  private decodeJwtPayload(token: string): any {
    // JWT format: header.payload.signature (base64url)
    const parts = token.split('.')
    if (parts.length < 2) return null

    const base64Url = parts[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64.padEnd(base64.length + (4 - (base64.length % 4)) % 4, '=')
    const json = atob(padded)
    return JSON.parse(json)
  }
}
