import { Component, inject, Input, OnInit, OnDestroy } from '@angular/core'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap'
import { Subject, forkJoin, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs'
import { UserAvatarComponent } from '@/app/components/user-avatar/user-avatar.component'
import {
  ResourcePermissionService,
  type ResourcePermission,
} from '@/app/services/resource-permission.service'
import { MasterService } from '@/app/services/master.service'
import { EnvService } from '@/app/services/env.service'
import { AuthService } from '@/app/services/auth.service'
import { removeDiacritics } from '@/app/shared/SharedFunction'

interface UserOption {
  id: string
  userName: string
  name: string
  /** Phòng ban/chức danh (từ SSO) hiển thị phụ dưới username — rỗng nếu chưa liên kết SSO. */
  orgLabel: string
}

/** Item trong danh mục đã fetch 1 lần — searchKey đã xoá dấu, gộp sẵn mọi field tìm được. */
interface DirectoryEntry extends UserOption {
  searchKey: string
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
            name="shareUserSearch"
            autocomplete="off"
            placeholder="Tìm theo tên, username, email, SĐT, phòng ban, chức danh..."
            [(ngModel)]="searchText"
            (input)="search$.next(searchText)"
            (focus)="onSearchFocus()"
            (blur)="onSearchBlur()"
          />
          @if (userResults.length > 0) {
            <div class="share-user-dropdown" (mousedown)="$event.preventDefault()">
              <div class="share-user-dropdown__header">
                <div class="d-flex align-items-center gap-2" style="cursor: pointer" (click)="toggleSelectAll()">
                  <input type="checkbox" class="form-check-input" [checked]="isAllSelected()" />
                  <span class="fw-medium small">Chọn tất cả ({{ userResults.length }})</span>
                </div>
                @if (selectedToAdd.size > 0) {
                  <div class="d-flex gap-2">
                    <button type="button" class="btn btn-sm btn-light" (click)="clearSelection()">Bỏ chọn</button>
                    <button type="button" class="btn btn-sm btn-primary" (click)="addSelectedUsers()">
                      Thêm {{ selectedToAdd.size }}
                    </button>
                  </div>
                }
              </div>
              @for (user of userResults; track user.id) {
                <div class="share-user-dropdown__item" (click)="toggleSelect(user)">
                  <input type="checkbox" class="form-check-input me-2" [checked]="selectedToAdd.has(user.id)" />
                  <app-user-avatar [userId]="user.id" [name]="user.name" [size]="28" />
                  <div class="ms-2">
                    <div class="fw-medium small">{{ user.name }}</div>
                    <div class="text-muted" style="font-size: 0.75rem">
                      {{ user.userName }}@if (user.orgLabel) { <span> · {{ user.orgLabel }}</span> }
                    </div>
                  </div>
                </div>
              }
            </div>
          }
        </div>
      </div>

      <div class="mb-2 d-flex justify-content-between align-items-center">
        <label class="form-label fw-semibold mb-0">Người có quyền truy cập</label>
        @if (permissions.length > 0) {
          <div class="d-flex align-items-center gap-2">
            <div class="form-check mb-0">
              <input
                type="checkbox"
                class="form-check-input"
                id="selectAllPerms"
                [checked]="isAllPermsSelected()"
                (change)="toggleSelectAllPerms()"
              />
              <label class="form-check-label small" for="selectAllPerms">Chọn tất cả</label>
            </div>
            @if (selectedPermIds.size > 0) {
              <button type="button" class="btn btn-sm btn-outline-danger" (click)="removeSelectedPerms()">
                <i class="ti ti-trash me-1"></i>Xoá ({{ selectedPermIds.size }})
              </button>
            }
          </div>
        }
      </div>

      @if (loading) {
        <div class="text-center py-3">
          <div class="spinner-border spinner-border-sm text-primary"></div>
        </div>
      }

      @for (perm of permissions; track perm.id) {
        <div class="d-flex align-items-center py-2 border-bottom">
          <input
            type="checkbox"
            class="form-check-input me-2"
            [checked]="selectedPermIds.has(perm.id)"
            (change)="togglePermSelect(perm.id)"
          />
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
    .share-user-dropdown__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 8px;
      padding: 8px 12px;
      border-bottom: 1px solid var(--greeva-border-color);
      background: var(--greeva-tertiary-bg);
      position: sticky;
      top: 0;
      z-index: 1;
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
  /** Ô tìm có đang focus không — dropdown chỉ hiện khi true, tránh che nội dung phía dưới lúc vừa mở modal. */
  private isSearchFocused = false
  /** Id các user đang được tích chọn trong dropdown "Thêm người" — chờ bấm "Thêm" mới gọi API (hỗ trợ chọn nhiều/chọn tất cả). */
  selectedToAdd = new Set<string>()
  /** Id các permission đang được tích chọn trong danh sách "Người có quyền truy cập" — hỗ trợ xoá nhiều cùng lúc. */
  selectedPermIds = new Set<string>()

  /** Toàn bộ user (Cloud + org/chức danh từ SSO) — fetch 1 lần khi mở modal, lọc phía client. */
  private directory: DirectoryEntry[] = []

  ngOnInit(): void {
    this.loadPermissions()
    this.loadDirectory()

    this.search$
      .pipe(
        debounceTime(150),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((term) => {
        this.userResults = this.filterDirectory(term)
      })
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  private loadDirectory(): void {
    this.master.get<any>(`${this.env.apiUrl}/api/users/directory`).subscribe({
      next: (res: any) => {
        const items = res?.data ?? []
        this.directory = items.map((u: any) => {
          const orgTitles: string[] = u.orgTitles ?? []
          const orgShortNames: string[] = u.orgShortNames ?? []
          const orgRoleNames: string[] = u.orgRoleNames ?? []
          const orgLabel = [...orgTitles, ...orgRoleNames].filter(Boolean).join(' · ')
          const rawKey = [u.name, u.userName, u.email, u.phone, ...orgTitles, ...orgShortNames, ...orgRoleNames]
            .filter(Boolean)
            .join(' ')

          return {
            id: u.id,
            userName: u.userName,
            name: u.name,
            orgLabel,
            searchKey: removeDiacritics(rawKey),
          }
        })
        // Chỉ tự hiện danh sách nếu ô tìm đang được focus sẵn (vd. directory load chậm hơn thao tác click).
        // Không tự hiện khi vừa mở modal — tránh che nội dung phía dưới lúc chưa ai bấm vào ô tìm.
        if (this.isSearchFocused) this.userResults = this.filterDirectory(this.searchText)
      },
    })
  }

  /** Chưa gõ gì → hiện toàn bộ danh mục. Có gõ → lọc theo key. Cả 2 trường hợp đều KHÔNG giới hạn số lượng
   * (trừ người đã có quyền/chính mình) — khung kết quả tự cuộn (overflow-y: auto). */
  private filterDirectory(term: string): UserOption[] {
    const excludeIds = new Set([
      ...this.permissions.map((p) => p.userId),
      this.currentUserId,
    ])
    const available = this.directory.filter((u) => !excludeIds.has(u.id))

    const trimmed = term?.trim()
    const matched = trimmed
      ? available.filter((u) => u.searchKey.includes(removeDiacritics(trimmed)))
      : available

    return matched.map(({ id, userName, name, orgLabel }) => ({ id, userName, name, orgLabel }))
  }

  /** Bấm vào ô tìm khi chưa gõ gì — hiện ngay toàn bộ danh mục để duyệt. */
  onSearchFocus(): void {
    this.isSearchFocused = true
    this.userResults = this.filterDirectory(this.searchText)
  }

  /** Rời khỏi ô tìm — ẩn dropdown. Delay nhẹ để click chọn 1 item (bên trong dropdown) kịp xử lý trước. */
  onSearchBlur(): void {
    this.isSearchFocused = false
    setTimeout(() => {
      if (!this.isSearchFocused) this.userResults = []
    }, 150)
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

  /** Tích/bỏ tích 1 người trong dropdown gợi ý. */
  toggleSelect(user: UserOption): void {
    if (this.selectedToAdd.has(user.id)) this.selectedToAdd.delete(user.id)
    else this.selectedToAdd.add(user.id)
  }

  isAllSelected(): boolean {
    return this.userResults.length > 0 && this.userResults.every((u) => this.selectedToAdd.has(u.id))
  }

  /** Chọn/bỏ chọn tất cả người đang hiển thị trong dropdown (theo kết quả lọc hiện tại). */
  toggleSelectAll(): void {
    if (this.isAllSelected()) {
      this.userResults.forEach((u) => this.selectedToAdd.delete(u.id))
    } else {
      this.userResults.forEach((u) => this.selectedToAdd.add(u.id))
    }
  }

  clearSelection(): void {
    this.selectedToAdd.clear()
    this.closeDropdown()
  }

  /** Thêm quyền cho toàn bộ người đã tích chọn — gọi song song rồi reload 1 lần. */
  addSelectedUsers(): void {
    const ids = Array.from(this.selectedToAdd)
    if (ids.length === 0) return

    const requests = ids.map((userId) =>
      this.permService.setPermission({
        resourceType: this.resourceType,
        resourceId: this.resourceId,
        userId,
        canView: true,
        canAdd: false,
        canEdit: false,
        canDelete: false,
      })
    )

    forkJoin(requests).subscribe(() => {
      this.selectedToAdd.clear()
      this.searchText = ''
      this.loadPermissions()
      this.closeDropdown()
    })
  }

  /** Đóng gợi ý sau khi hoàn tất thao tác (thêm/bỏ chọn) — tránh danh sách đầy đủ cứ hiện mãi, che nội dung. */
  private closeDropdown(): void {
    this.isSearchFocused = false
    this.userResults = []
  }

  togglePermSelect(permId: string): void {
    if (this.selectedPermIds.has(permId)) this.selectedPermIds.delete(permId)
    else this.selectedPermIds.add(permId)
  }

  isAllPermsSelected(): boolean {
    return this.permissions.length > 0 && this.permissions.every((p) => this.selectedPermIds.has(p.id))
  }

  toggleSelectAllPerms(): void {
    if (this.isAllPermsSelected()) {
      this.selectedPermIds.clear()
    } else {
      this.permissions.forEach((p) => this.selectedPermIds.add(p.id))
    }
  }

  removePerm(perm: ResourcePermission): void {
    this.permService.removePermission(perm.id).subscribe({
      next: (res) => {
        if (res?.isSuccess) {
          this.permissions = this.permissions.filter((p) => p.id !== perm.id)
          this.selectedPermIds.delete(perm.id)
        }
      },
    })
  }

  /** Xoá quyền của toàn bộ người đã tích chọn — gọi song song rồi cập nhật danh sách 1 lần. */
  removeSelectedPerms(): void {
    const ids = Array.from(this.selectedPermIds)
    if (ids.length === 0) return

    forkJoin(ids.map((id) => this.permService.removePermission(id))).subscribe(() => {
      this.permissions = this.permissions.filter((p) => !ids.includes(p.id))
      this.selectedPermIds.clear()
    })
  }
}
