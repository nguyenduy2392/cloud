import { Component, inject, Input, OnInit, OnDestroy } from '@angular/core'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap'
import { Subject, takeUntil, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs'
import { UserAvatarComponent } from '@/app/components/user-avatar/user-avatar.component'
import {
  ResourcePermissionService,
  type ResourcePermission,
} from '@/app/services/resource-permission.service'
import { MasterService } from '@/app/services/master.service'
import { EnvService } from '@/app/services/env.service'
import { AuthService } from '@/app/services/auth.service'

interface UserOption {
  id: string
  userName: string
  name: string
}

@Component({
  selector: 'app-share-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, UserAvatarComponent],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">
        <i class="ti ti-share me-2"></i>Chia sẻ "{{ resourceName }}"
      </h5>
      <button type="button" class="btn-close" (click)="activeModal.dismiss()"></button>
    </div>
    <div class="modal-body">
      <div class="mb-3">
        <label class="form-label fw-semibold">Thêm người</label>
        <div class="position-relative">
          <input
            type="text"
            class="form-control"
            placeholder="Tìm theo tên hoặc username..."
            [(ngModel)]="searchText"
            (input)="search$.next(searchText)"
          />
          @if (userResults.length > 0 && searchText) {
            <div class="share-user-dropdown">
              @for (user of userResults; track user.id) {
                <div class="share-user-dropdown__item" (click)="addUser(user)">
                  <app-user-avatar [userId]="user.id" [name]="user.name" [size]="28" />
                  <div class="ms-2">
                    <div class="fw-medium small">{{ user.name }}</div>
                    <div class="text-muted" style="font-size: 0.75rem">{{ user.userName }}</div>
                  </div>
                </div>
              }
            </div>
          }
        </div>
      </div>

      <div class="mb-2">
        <label class="form-label fw-semibold">Người có quyền truy cập</label>
      </div>

      @if (loading) {
        <div class="text-center py-3">
          <div class="spinner-border spinner-border-sm text-primary"></div>
        </div>
      }

      @for (perm of permissions; track perm.id) {
        <div class="d-flex align-items-center py-2 border-bottom">
          <app-user-avatar [userId]="perm.userId" [name]="perm.name" [size]="32" />
          <div class="ms-2 flex-grow-1">
            <div class="fw-medium small">{{ perm.name }}</div>
            <div class="text-muted" style="font-size: 0.75rem">{{ perm.userName }}</div>
          </div>
          <span class="text-muted small me-2">Xem</span>
          <button
            class="btn btn-sm btn-light"
            (click)="removePerm(perm)"
            title="Xoá quyền"
          >
            <i class="ti ti-trash text-danger"></i>
          </button>
        </div>
      }

      @if (!loading && permissions.length === 0) {
        <p class="text-muted small text-center py-3">Chưa chia sẻ với ai.</p>
      }
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-light" (click)="activeModal.dismiss()">Đóng</button>
    </div>
  `,
  styles: `
    .share-user-dropdown {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      background: var(--greeva-body-bg);
      border: 1px solid var(--greeva-border-color);
      border-radius: 6px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.1);
      z-index: 10;
      max-height: 200px;
      overflow-y: auto;
    }
    .share-user-dropdown__item {
      display: flex;
      align-items: center;
      padding: 8px 12px;
      cursor: pointer;
    }
    .share-user-dropdown__item:hover {
      background: var(--greeva-tertiary-bg);
    }
  `,
})
export class ShareModalComponent implements OnInit, OnDestroy {
  readonly activeModal = inject(NgbActiveModal)
  private permService = inject(ResourcePermissionService)
  private master = inject(MasterService)
  private env = inject(EnvService)
  private authService = inject(AuthService)
  private currentUserId = this.authService.getStoredUser()?.id ?? ''
  private destroy$ = new Subject<void>()

  @Input() resourceId = ''
  @Input() resourceType = 0
  @Input() resourceName = ''

  loading = false
  permissions: ResourcePermission[] = []
  searchText = ''
  userResults: UserOption[] = []
  search$ = new Subject<string>()

  ngOnInit(): void {
    this.loadPermissions()

    this.search$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((term) => {
          if (!term?.trim()) return of([])
          return this.searchUsers(term.trim())
        }),
        takeUntil(this.destroy$)
      )
      .subscribe((users) => {
        this.userResults = users
      })
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  private searchUsers(keyword: string) {
    return this.master.getwithParam<any>(
      `${this.env.apiUrl}/api/users`,
      { keyword, pageSize: 10 }
    )
    .pipe(
      switchMap((res: any) => {
        const items: UserOption[] = (res?.data?.items ?? []).map((u: any) => ({
          id: u.id,
          userName: u.userName,
          name: u.name,
        }))
        const excludeIds = new Set([
          ...this.permissions.map((p) => p.userId),
          this.currentUserId,
        ])
        return of(items.filter((u) => !excludeIds.has(u.id)))
      })
    )
  }

  private loadPermissions(): void {
    this.loading = true
    this.permService.getPermissions(this.resourceType, this.resourceId).subscribe({
      next: (res) => {
        this.loading = false
        if (res?.isSuccess) {
          this.permissions = res.data ?? []
        }
      },
      error: () => {
        this.loading = false
      },
    })
  }

  addUser(user: UserOption): void {
    if (this.permissions.some((p) => p.userId === user.id)) {
      this.searchText = ''
      this.userResults = []
      return
    }

    this.permService
      .setPermission({
        resourceType: this.resourceType,
        resourceId: this.resourceId,
        userId: user.id,
        canView: true,
        canAdd: false,
        canEdit: false,
        canDelete: false,
      })
      .subscribe({
        next: (res) => {
          if (res?.isSuccess) this.loadPermissions()
        },
      })

    this.searchText = ''
    this.userResults = []
  }

  removePerm(perm: ResourcePermission): void {
    this.permService.removePermission(perm.id).subscribe({
      next: (res) => {
        if (res?.isSuccess) {
          this.permissions = this.permissions.filter((p) => p.id !== perm.id)
        }
      },
    })
  }
}
