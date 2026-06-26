import { Injectable } from '@angular/core'
import { EnvService } from './env.service'
import { MasterService } from './master.service'
import { Observable } from 'rxjs'
import { type ApiResponse } from './folder.service'

export interface ResourcePermission {
  id: string
  resourceType: number
  resourceId: string
  userId?: string | null
  userName?: string | null
  permission: number
  createdAt: string
}

export interface SetPermissionPayload {
  resourceType: number
  resourceId: string
  userId: string
  permission: number
}

@Injectable({ providedIn: 'root' })
export class CloudPermissionService {
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

  setPermission(dto: SetPermissionPayload): Observable<ApiResponse<unknown>> {
    return this.master.post<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/resource-permissions`,
      dto
    )
  }

  removePermission(id: string): Observable<ApiResponse<unknown>> {
    return this.master.delete<ApiResponse<unknown>>(
      `${this.env.apiUrl}/api/resource-permissions/${id}`
    )
  }
}
