import { Injectable } from '@angular/core'
import { Observable } from 'rxjs'
import { EnvService } from './env.service'
import { MasterService } from './master.service'
import { type ApiResponse } from './folder.service'

export interface TrashItem {
  id: string
  name: string
  type: 'file' | 'folder'
  deletedAt: string
  expiresAt: string
  sizeInBytes: number
  extension?: string | null
}

export interface RestoreResult {
  id: string
  mergedIntoId?: string | null
  mergedIntoName?: string | null
}

@Injectable({ providedIn: 'root' })
export class TrashService {
  constructor(
    private master: MasterService,
    private env: EnvService
  ) {}

  getAll(): Observable<ApiResponse<TrashItem[]>> {
    return this.master.get<ApiResponse<TrashItem[]>>(`${this.env.apiUrl}/api/trash`)
  }

  restore(id: string): Observable<ApiResponse<RestoreResult>> {
    return this.master.post<ApiResponse<RestoreResult>>(
      `${this.env.apiUrl}/api/trash/${id}/restore`,
      {}
    )
  }

  permanentDelete(id: string): Observable<ApiResponse<unknown>> {
    return this.master.delete<ApiResponse<unknown>>(`${this.env.apiUrl}/api/trash/${id}`)
  }

  emptyTrash(): Observable<ApiResponse<unknown>> {
    return this.master.delete<ApiResponse<unknown>>(`${this.env.apiUrl}/api/trash`)
  }
}
