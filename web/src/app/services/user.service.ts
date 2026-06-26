import { Injectable } from '@angular/core'
import { EnvService } from './env.service'
import { MasterService } from './master.service'
import { Observable } from 'rxjs'

/** Khớp `Core.Common.Response` */
export interface UserApiResponse<T = unknown> {
  isSuccess: boolean
  errorCode: number
  message: string
  data: T
}

export interface UserListItem {
  id: string
  userName: string
  name: string
  email?: string | null
  phone?: string | null
  address?: string | null
  avatar?: string | null
  birthday?: string | null
  gender?: number | null
  description?: string | null
  lastLogin?: string | null
  isRootAdmin: boolean
  isEmployee: boolean
  createdAt: string
}

export interface UserPagedResult {
  currentPage: number
  totalPages: number
  pageSize: number
  totalCount: number
  items: UserListItem[]
}

export interface CreateUserPayload {
  userName: string
  password: string
  name: string
  email?: string | null
  phone?: string | null
  address?: string | null
  birthday?: string | null
  gender?: number | null
  description?: string | null
  avatar?: string | null
  isRootAdmin?: boolean
  isEmployee?: boolean
}

export interface UpdateUserPayload {
  id: string
  name: string
  email?: string | null
  phone?: string | null
  address?: string | null
  birthday?: string | null
  gender?: number | null
  description?: string | null
  avatar?: string | null
  isRootAdmin?: boolean
  isEmployee?: boolean
}

export interface ChangePasswordPayload {
  userId: string
  currentPassword: string
  newPassword: string
}

export interface ResetPasswordPayload {
  userId: string
  newPassword: string
}

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(
    private master: MasterService,
    private env: EnvService
  ) {}

  getPaged(params: {
    page: number
    pageSize: number
    keyword?: string
  }): Observable<UserApiResponse<UserPagedResult>> {
    const q: Record<string, string | number> = {
      page: params.page,
      pageSize: params.pageSize,
    }
    const kw = params.keyword?.trim()
    if (kw) q['keyword'] = kw
    return this.master.getwithParam<UserApiResponse<UserPagedResult>>(
      `${this.env.apiUrl}/api/users`,
      q
    )
  }

  getUsersPaged(
    page: number,
    pageSize: number,
    keyword: string | null
  ): Observable<UserApiResponse<UserPagedResult>> {
    return this.getPaged({ page, pageSize, keyword: keyword ?? undefined })
  }

  /** Lấy toàn bộ user (không phân trang, dùng cho dropdown/chọn người phụ trách). */
  getAll(): Observable<UserApiResponse<UserListItem[]>> {
    return this.master.get<UserApiResponse<UserListItem[]>>(
      `${this.env.apiUrl}/api/users/all`
    )
  }

  getById(id: string): Observable<UserApiResponse<UserListItem>> {
    return this.master.get<UserApiResponse<UserListItem>>(
      `${this.env.apiUrl}/api/users/${id}`
    )
  }

  create(body: CreateUserPayload): Observable<UserApiResponse<unknown>> {
    return this.master.post<UserApiResponse<unknown>>(
      `${this.env.apiUrl}/api/users`,
      body
    )
  }

  update(id: string, body: UpdateUserPayload): Observable<UserApiResponse<unknown>> {
    return this.master.put<UserApiResponse<unknown>>(
      `${this.env.apiUrl}/api/users/${id}`,
      body
    )
  }

  delete(id: string): Observable<UserApiResponse<unknown>> {
    return this.master.delete<UserApiResponse<unknown>>(
      `${this.env.apiUrl}/api/users/${id}`
    )
  }

  bulkDelete(ids: string[]): Observable<UserApiResponse<{ deleted: number }>> {
    return this.master.post<UserApiResponse<{ deleted: number }>>(
      `${this.env.apiUrl}/api/users/bulk-delete`,
      ids
    )
  }

  changePassword(body: ChangePasswordPayload): Observable<UserApiResponse<unknown>> {
    return this.master.post<UserApiResponse<unknown>>(
      `${this.env.apiUrl}/api/users/change-password`,
      body
    )
  }

  resetPassword(body: ResetPasswordPayload): Observable<UserApiResponse<unknown>> {
    return this.master.post<UserApiResponse<unknown>>(
      `${this.env.apiUrl}/api/users/reset-password`,
      body
    )
  }

  uploadAvatar(userId: string, file: File): Observable<UserApiResponse<unknown>> {
    const formData = new FormData()
    formData.append('file', file)
    return this.master.postFormData<UserApiResponse<unknown>>(
      `${this.env.apiUrl}/api/users/${userId}/avatar`,
      formData
    )
  }

  deleteAvatar(userId: string): Observable<UserApiResponse<unknown>> {
    return this.master.delete<UserApiResponse<unknown>>(
      `${this.env.apiUrl}/api/users/${userId}/avatar`
    )
  }
}
