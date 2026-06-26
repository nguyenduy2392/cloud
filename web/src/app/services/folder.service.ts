import { Injectable } from '@angular/core'
import { EnvService } from './env.service'
import { MasterService } from './master.service'
import { Observable } from 'rxjs'

export interface ApiResponse<T = unknown> {
  isSuccess: boolean
  errorCode: number
  message: string
  data: T
}

export interface FolderItem {
  id: string
  name: string
  parentId?: string | null
  sizeInBytes: number
  createdAt: string
  createdByName?: string | null
  subFolderCount?: number
  fileCount?: number
}

export interface FileItem {
  id: string
  name: string
  folderId?: string | null
  sizeInBytes: number
  contentType?: string | null
  extension?: string | null
  createdAt: string
  createdByName?: string | null
}

export interface FolderContents {
  folder?: FolderItem | null
  breadcrumbs: FolderItem[]
  folders: FolderItem[]
  files: FileItem[]
  currentPage: number
  totalPages: number
  pageSize: number
  totalCount: number
}

export interface FolderTreeNode {
  id: string
  name: string
  parentId?: string | null
  children: FolderTreeNode[]
}

@Injectable({ providedIn: 'root' })
export class FolderService {
  constructor(
    private master: MasterService,
    private env: EnvService
  ) {}

  getContents(
    parentId?: string,
    page = 1,
    pageSize = 50,
    keyword?: string
  ): Observable<ApiResponse<FolderContents>> {
    const params: Record<string, string | number> = { page, pageSize }
    if (parentId) params['parentId'] = parentId
    if (keyword) params['keyword'] = keyword
    return this.master.getwithParam<ApiResponse<FolderContents>>(
      `${this.env.apiUrl}/api/folders`,
      params
    )
  }

  getTree(): Observable<ApiResponse<FolderTreeNode[]>> {
    return this.master.get<ApiResponse<FolderTreeNode[]>>(
      `${this.env.apiUrl}/api/folders/tree`
    )
  }

  getById(id: string): Observable<ApiResponse<FolderItem>> {
    return this.master.get<ApiResponse<FolderItem>>(
      `${this.env.apiUrl}/api/folders/${id}`
    )
  }

  create(name: string, parentId?: string): Observable<ApiResponse<FolderItem>> {
    return this.master.post<ApiResponse<FolderItem>>(
      `${this.env.apiUrl}/api/folders`,
      { name, parentId: parentId ?? null }
    )
  }

  rename(id: string, name: string): Observable<ApiResponse<unknown>> {
    return this.master.put<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/folders/${id}/rename`,
      { name }
    )
  }

  move(id: string, parentId?: string): Observable<ApiResponse<unknown>> {
    return this.master.put<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/folders/${id}/move`,
      { parentId: parentId ?? null }
    )
  }

  delete(id: string): Observable<ApiResponse<unknown>> {
    return this.master.delete<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/folders/${id}`
    )
  }
}
