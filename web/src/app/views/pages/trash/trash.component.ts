import { CommonModule, DatePipe } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { NgbModal, NgbModalModule } from '@ng-bootstrap/ng-bootstrap'
import { TableLoadingOverlayComponent } from '@components/table-loading-overlay/table-loading-overlay.component'
import { AppEmptyStateComponent } from '@components/empty-state/empty-state.component'
import { ConfirmDialogModalComponent } from '@components/confirm-dialog-modal/confirm-dialog-modal.component'
import { showErrorModal } from '@/app/shared/SharedFunction'
import { FileSizePipe } from '@/app/shared/file-size.pipe'
import { TrashService, type TrashItem } from '@/app/services/trash.service'
import { PageTitle } from '@/app/components/page-title'
import { UserStorageService } from '@/app/services/user-storage.service'

@Component({
  selector: 'app-trash',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    NgbModalModule,
    TableLoadingOverlayComponent,
    AppEmptyStateComponent,
    FileSizePipe,
    PageTitle,
  ],
  templateUrl: './trash.component.html',
})
export class TrashComponent implements OnInit {
  loading = false
  items: TrashItem[] = []
  restoring = new Set<string>()
  deleting = new Set<string>()
  emptyingTrash = false

  constructor(
    private trashService: TrashService,
    private userStorageService: UserStorageService,
    private ngbModal: NgbModal
  ) {}

  ngOnInit(): void {
    this.load()
  }

  load(): void {
    this.loading = true
    this.trashService.getAll().subscribe({
      next: (res) => {
        this.loading = false
        if (!res?.isSuccess) {
          this.showError('Lỗi', res?.message ?? 'Không tải được thùng rác.')
          return
        }
        this.items = (res.data as TrashItem[]) ?? []
      },
      error: (err) => {
        this.loading = false
        this.showError('Lỗi', err?.error?.message ?? 'Lỗi kết nối máy chủ.')
      },
    })
  }

  restore(item: TrashItem): void {
    this.restoring.add(item.id)
    this.trashService.restore(item.id).subscribe({
      next: (res) => {
        this.restoring.delete(item.id)
        if (!res?.isSuccess) {
          this.showError('Lỗi khôi phục', res?.message ?? 'Không khôi phục được.')
          return
        }
        this.items = this.items.filter((i) => i.id !== item.id)
      },
      error: (err) => {
        this.restoring.delete(item.id)
        this.showError('Lỗi khôi phục', err?.error?.message ?? 'Lỗi kết nối máy chủ.')
      },
    })
  }

  permanentDelete(item: TrashItem): void {
    const ref = this.ngbModal.open(ConfirmDialogModalComponent, {
      centered: true,
      backdrop: 'static',
    })
    const dlg = ref.componentInstance
    dlg.title = 'Xóa vĩnh viễn'
    dlg.message = `Xóa vĩnh viễn "${item.name}"? Hành động này không thể hoàn tác.`
    dlg.confirmLabel = 'Xóa vĩnh viễn'
    dlg.danger = true
    ref.closed.subscribe((confirmed: boolean) => {
      if (confirmed !== true) return
      this.deleting.add(item.id)
      this.trashService.permanentDelete(item.id).subscribe({
        next: (res) => {
          this.deleting.delete(item.id)
          if (!res?.isSuccess) {
            this.showError('Lỗi xóa', res?.message ?? 'Không xóa được.')
            return
          }
          this.items = this.items.filter((i) => i.id !== item.id)
          this.userStorageService.refresh()
        },
        error: (err) => {
          this.deleting.delete(item.id)
          this.showError('Lỗi xóa', err?.error?.message ?? 'Lỗi kết nối máy chủ.')
        },
      })
    })
  }

  emptyTrash(): void {
    if (this.items.length === 0) return
    const ref = this.ngbModal.open(ConfirmDialogModalComponent, {
      centered: true,
      backdrop: 'static',
    })
    const dlg = ref.componentInstance
    dlg.title = 'Dọn sạch thùng rác'
    dlg.message = `Xóa vĩnh viễn tất cả ${this.items.length} mục trong thùng rác? Hành động này không thể hoàn tác.`
    dlg.confirmLabel = 'Dọn sạch'
    dlg.danger = true
    ref.closed.subscribe((confirmed: boolean) => {
      if (confirmed !== true) return
      this.emptyingTrash = true
      this.trashService.emptyTrash().subscribe({
        next: (res) => {
          this.emptyingTrash = false
          if (!res?.isSuccess) {
            this.showError('Lỗi', res?.message ?? 'Không dọn sạch được.')
            return
          }
          this.items = []
          this.userStorageService.refresh()
        },
        error: (err) => {
          this.emptyingTrash = false
          this.showError('Lỗi', err?.error?.message ?? 'Lỗi kết nối máy chủ.')
        },
      })
    })
  }

  daysUntilExpiry(expiresAt: string): number {
    const diff = new Date(expiresAt).getTime() - Date.now()
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)))
  }

  getIcon(item: TrashItem): string {
    if (item.type === 'folder') return 'ti ti-folder text-warning'
    const ext = (item.extension ?? '').replace('.', '').toLowerCase()
    const map: Record<string, string> = {
      pdf: 'ti ti-file-type-pdf text-danger',
      doc: 'ti ti-file-type-doc text-primary',
      docx: 'ti ti-file-type-docx text-primary',
      xls: 'ti ti-file-type-xls text-success',
      xlsx: 'ti ti-file-type-xls text-success',
      jpg: 'ti ti-file-type-jpg text-info',
      jpeg: 'ti ti-file-type-jpg text-info',
      png: 'ti ti-file-type-png text-info',
      mp4: 'ti ti-player-play text-purple',
      zip: 'ti ti-file-zip text-secondary',
    }
    return map[ext] ?? 'ti ti-file text-muted'
  }

  private showError(title: string, message: string): void {
    showErrorModal(this.ngbModal, title, message)
  }
}
