import { Injectable } from '@angular/core'
import { EnvService } from './env.service'
import { MasterService } from './master.service'
import { Observable, Subject, shareReplay, startWith, switchMap } from 'rxjs'
import { type ApiResponse } from './folder.service'

export interface UserStorage {
  userId: string
  usedBytes: number
  maxBytes: number
}

@Injectable({ providedIn: 'root' })
export class UserStorageService {
  private refresh$ = new Subject<void>()

  storage$: Observable<ApiResponse<UserStorage>> = this.refresh$.pipe(
    startWith(undefined),
    switchMap(() => this.getMyStorage()),
    shareReplay(1)
  )

  constructor(
    private master: MasterService,
    private env: EnvService
  ) {}

  refresh(): void {
    this.refresh$.next()
  }

  getMyStorage(): Observable<ApiResponse<UserStorage>> {
    return this.master.get<ApiResponse<UserStorage>>(
      `${this.env.apiUrl}/api/user-storage/me`
    )
  }

  getUserStorage(userId: string): Observable<ApiResponse<UserStorage>> {
    return this.master.get<ApiResponse<UserStorage>>(
      `${this.env.apiUrl}/api/user-storage/${userId}`
    )
  }

  setQuota(userId: string, maxBytes: number): Observable<ApiResponse<unknown>> {
    return this.master.put<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/user-storage/${userId}/quota`,
      { maxBytes }
    )
  }
}
