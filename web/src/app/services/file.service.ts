import { Injectable } from '@angular/core'
import { EnvService } from './env.service'
import { MasterService } from './master.service'
import { Observable } from 'rxjs'
import { type ApiResponse, type FileItem } from './folder.service'

@Injectable({ providedIn: 'root' })
export class CloudFileService {
  constructor(
    private master: MasterService,
    private env: EnvService
  ) {}

  getById(id: string): Observable<ApiResponse<FileItem>> {
    return this.master.get<ApiResponse<FileItem>>(
      `${this.env.apiUrl}/api/files/${id}`
    )
  }

  download(id: string): Observable<Blob> {
    return this.master.getBlob(
      `${this.env.apiUrl}/api/files/${id}/download`
    )
  }

  upload(file: File, folderId?: string): Observable<ApiResponse<FileItem>> {
    const formData = new FormData()
    formData.append('file', file)
    if (folderId) formData.append('folderId', folderId)
    return this.master.postFormData<ApiResponse<FileItem>>(
      `${this.env.apiUrl}/api/files/upload`,
      formData
    )
  }

  rename(id: string, name: string): Observable<ApiResponse<unknown>> {
    return this.master.put<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/files/${id}/rename`,
      { name }
    )
  }

  move(id: string, folderId?: string): Observable<ApiResponse<unknown>> {
    return this.master.put<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/files/${id}/move`,
      { folderId: folderId ?? null }
    )
  }

  delete(id: string): Observable<ApiResponse<unknown>> {
    return this.master.delete<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/files/${id}`
    )
  }

  bulkDelete(ids: string[]): Observable<ApiResponse<{ deleted: number }>> {
    return this.master.post<ApiResponse<{ deleted: number }>>(
      `${this.env.apiUrl}/api/files/bulk-delete`,
      ids
    )
  }
}
