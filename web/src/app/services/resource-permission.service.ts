import { Injectable } from '@angular/core'
import { Observable } from 'rxjs'
import { EnvService } from './env.service'
import { MasterService } from './master.service'
import { type ApiResponse, type FolderContents } from './folder.service'

export interface ResourcePermission {
  id: string
  resourceType: number
  resourceId: string
  userId: string
  userName: string
  name: string
  canView: boolean
  canAdd: boolean
  canEdit: boolean
  canDelete: boolean
}

export interface SetPermissionRequest {
  resourceType: number
  resourceId: string
  userId: string
  canView: boolean
  canAdd: boolean
  canEdit: boolean
  canDelete: boolean
}

@Injectable({ providedIn: 'root' })
export class ResourcePermissionService {
  constructor(
    private master: MasterService,
    private env: EnvService
  ) {}

  getPermissions(
    resourceType: number,
    resourceId: string
  ): Observable<ApiResponse<ResourcePermission[]>> {
    return this.master.getwithParam<ApiResponse<ResourcePermission[]>>(
      `${this.env.apiUrl}/api/resource-permissions`,
      { resourceType, resourceId }
    )
  }

  setPermission(
    dto: SetPermissionRequest
  ): Observable<ApiResponse<{ id: string }>> {
    return this.master.post<ApiResponse<{ id: string }>>(
      `${this.env.apiUrl}/api/resource-permissions`,
      dto
    )
  }

  removePermission(id: string): Observable<ApiResponse<unknown>> {
    return this.master.delete<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/resource-permissions/${id}`
    )
  }

  getSharedWithMe(
    page = 1,
    pageSize = 40,
    keyword?: string
  ): Observable<ApiResponse<FolderContents>> {
    const params: Record<string, string | number> = { page, pageSize }
    if (keyword) params['keyword'] = keyword
    return this.master.getwithParam<ApiResponse<FolderContents>>(
      `${this.env.apiUrl}/api/resource-permissions/shared-with-me`,
      params
    )
  }
}
