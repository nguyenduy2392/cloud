import { CommonModule, DatePipe } from '@angular/common'
import { Component, OnInit, OnDestroy } from '@angular/core'
import { ActivatedRoute, Router, RouterModule } from '@angular/router'
import { FormsModule } from '@angular/forms'
import {
  NgbDropdownModule,
  NgbModal,
  NgbModalModule,
} from '@ng-bootstrap/ng-bootstrap'
import { Subject, takeUntil, map, distinctUntilChanged, filter, skip } from 'rxjs'
import { TableLoadingOverlayComponent } from '@components/table-loading-overlay/table-loading-overlay.component'
import { AppEmptyStateComponent } from '@components/empty-state/empty-state.component'
import { ConfirmDialogModalComponent } from '@components/confirm-dialog-modal/confirm-dialog-modal.component'
import { showErrorModal } from '@/app/shared/SharedFunction'
import { FileSizePipe } from '@/app/shared/file-size.pipe'
import {
  FolderService,
  type FolderItem,
  type FileItem,
} from '@/app/services/folder.service'
import { CloudFileService } from '@/app/services/file.service'
import { UserStorageService } from '@/app/services/user-storage.service'
import { UploadWorkerService } from '@/app/services/upload-worker.service'
import { CreateFolderModalComponent } from './create-folder-modal'
import { RenameModalComponent } from './rename-modal'

type FileIconInfo = { icon: string; colorClass: string }

const EXT_ICON_MAP: Record<string, FileIconInfo> = {
  pdf:  { icon: 'ti-file-type-pdf', colorClass: 'text-danger' },
  doc:  { icon: 'ti-file-type-doc', colorClass: 'text-primary' },
  docx: { icon: 'ti-file-type-docx', colorClass: 'text-primary' },
  xls:  { icon: 'ti-file-type-xls', colorClass: 'text-success' },
  xlsx: { icon: 'ti-file-type-xls', colorClass: 'text-success' },
  ppt:  { icon: 'ti-file-type-ppt', colorClass: 'text-warning' },
  pptx: { icon: 'ti-file-type-ppt', colorClass: 'text-warning' },
  jpg:  { icon: 'ti-file-type-jpg', colorClass: 'text-info' },
  jpeg: { icon: 'ti-file-type-jpg', colorClass: 'text-info' },
  png:  { icon: 'ti-file-type-png', colorClass: 'text-info' },
  gif:  { icon: 'ti-photo', colorClass: 'text-info' },
  svg:  { icon: 'ti-file-type-svg', colorClass: 'text-info' },
  mp4:  { icon: 'ti-player-play', colorClass: 'text-purple' },
  avi:  { icon: 'ti-player-play', colorClass: 'text-purple' },
  mov:  { icon: 'ti-player-play', colorClass: 'text-purple' },
  mp3:  { icon: 'ti-music', colorClass: 'text-purple' },
  zip:  { icon: 'ti-file-zip', colorClass: 'text-secondary' },
  rar:  { icon: 'ti-file-zip', colorClass: 'text-secondary' },
  '7z': { icon: 'ti-file-zip', colorClass: 'text-secondary' },
  txt:  { icon: 'ti-file-text', colorClass: 'text-muted' },
  csv:  { icon: 'ti-file-spreadsheet', colorClass: 'text-success' },
}

const DEFAULT_ICON: FileIconInfo = {
  icon: 'ti-file',
  colorClass: 'text-muted',
}

@Component({
  selector: 'app-explorer',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    DatePipe,
    TableLoadingOverlayComponent,
    AppEmptyStateComponent,
    NgbDropdownModule,
    NgbModalModule,
    FileSizePipe,
  ],
  templateUrl: './explorer.component.html',
  styles: `
    :host { display: block; }
    .folder-row { cursor: pointer; }
    .folder-row:hover { background-color: rgba(0,0,0,.03); }
    .file-icon { font-size: 1.25rem; }
    .folder-icon { font-size: 1.25rem; color: #f0ad4e; }
    .explorer-toolbar {
      position: sticky;
      top: 50px;
      z-index: 100;
      background: var(--greeva-body-bg);
      padding-top: 0.75rem;
      padding-bottom: 0.75rem;
      margin-bottom: 0 !important;
    }
    ::ng-deep .explorer-thead th {
      position: sticky;
      top: 99px;
      z-index: 99;
      background: var(--greeva-tertiary-bg);
    }
  `,
})
export class ExplorerComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>()

  loading = false
  currentFolderId?: string
  breadcrumbs: FolderItem[] = []
  folders: FolderItem[] = []
  files: FileItem[] = []

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private folderService: FolderService,
    private fileService: CloudFileService,
    private userStorageService: UserStorageService,
    private uploadWorker: UploadWorkerService,
    private ngbModal: NgbModal
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        this.currentFolderId = params.get('id') ?? undefined
        this.loadContents()
      })

    this.uploadWorker.tasks$
      .pipe(
        map((tasks) => ({
          hasActive: tasks.some((t) => t.status === 'pending' || t.status === 'uploading'),
          doneCount: tasks.filter((t) => t.status === 'done').length,
        })),
        distinctUntilChanged((a, b) => a.hasActive === b.hasActive && a.doneCount === b.doneCount),
        filter(({ hasActive, doneCount }) => !hasActive && doneCount > 0),
        takeUntil(this.destroy$)
      )
      .subscribe(() => this.loadContents())
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  get isEmpty(): boolean {
    return this.folders.length === 0 && this.files.length === 0
  }

  loadContents(): void {
    this.loading = true
    this.folderService.getContents(this.currentFolderId).subscribe({
      next: (res) => {
        this.loading = false
        if (!res?.isSuccess) {
          this.showError('Lỗi tải dữ liệu', res?.message ?? 'Không tải được nội dung thư mục.')
          return
        }
        const data: any = res.data
        this.breadcrumbs = data?.breadcrumbs ?? []
        this.folders = data?.folders?.items ?? data?.folders ?? []
        this.files = data?.files?.items ?? data?.files ?? []
      },
      error: (err) => {
        this.loading = false
        this.showError('Lỗi tải dữ liệu', err?.error?.message ?? 'Lỗi kết nối máy chủ.')
      },
    })
  }

  // --- Navigation ---

  openFolder(folder: FolderItem): void {
    this.router.navigate(['/folder', folder.id])
  }

  navigateBreadcrumb(crumb?: FolderItem): void {
    if (!crumb) {
      this.router.navigate(['/my-cloud'])
    } else {
      this.router.navigate(['/folder', crumb.id])
    }
  }

  // --- Create Folder ---

  openCreateFolder(): void {
    const ref = this.ngbModal.open(CreateFolderModalComponent, {
      centered: true,
      backdrop: 'static',
    })
    ref.closed.subscribe((name: string) => {
      if (!name) return
      this.folderService.create(name, this.currentFolderId).subscribe({
        next: (res) => {
          if (!res?.isSuccess) {
            this.showError('Lỗi tạo thư mục', res?.message ?? 'Không tạo được thư mục.')
            return
          }
          this.loadContents()
          this.userStorageService.refresh()
        },
        error: (err) => this.showError('Lỗi tạo thư mục', err?.error?.message ?? 'Lỗi kết nối máy chủ.'),
      })
    })
  }

  // --- File Upload ---

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement
    const files = input.files
    if (!files?.length) return
    for (let i = 0; i < files.length; i++) {
      this.uploadWorker.enqueue(files[i], this.currentFolderId)
    }
    input.value = ''
  }

  // --- Rename ---

  renameFolder(folder: FolderItem): void {
    const ref = this.ngbModal.open(RenameModalComponent, {
      centered: true,
      backdrop: 'static',
    })
    ref.componentInstance.currentName = folder.name
    ref.componentInstance.newName = folder.name
    ref.closed.subscribe((newName: string) => {
      if (!newName) return
      this.folderService.rename(folder.id, newName).subscribe({
        next: (res) => {
          if (!res?.isSuccess) {
            this.showError('Lỗi đổi tên', res?.message ?? 'Không đổi tên được.')
            return
          }
          this.loadContents()
        },
        error: (err) => this.showError('Lỗi đổi tên', err?.error?.message ?? 'Lỗi kết nối máy chủ.'),
      })
    })
  }

  renameFile(file: FileItem): void {
    const ref = this.ngbModal.open(RenameModalComponent, {
      centered: true,
      backdrop: 'static',
    })
    ref.componentInstance.currentName = file.name
    ref.componentInstance.newName = file.name
    ref.closed.subscribe((newName: string) => {
      if (!newName) return
      this.fileService.rename(file.id, newName).subscribe({
        next: (res) => {
          if (!res?.isSuccess) {
            this.showError('Lỗi đổi tên', res?.message ?? 'Không đổi tên được.')
            return
          }
          this.loadContents()
        },
        error: (err) => this.showError('Lỗi đổi tên', err?.error?.message ?? 'Lỗi kết nối máy chủ.'),
      })
    })
  }

  // --- Delete ---

  deleteFolder(folder: FolderItem): void {
    const ref = this.ngbModal.open(ConfirmDialogModalComponent, {
      backdrop: 'static',
      centered: true,
    })
    const dlg = ref.componentInstance
    dlg.title = 'Xóa thư mục'
    dlg.message = `Xóa thư mục "${folder.name}" và toàn bộ nội dung bên trong? Hành động này không thể hoàn tác.`
    dlg.confirmLabel = 'Xóa'
    dlg.danger = true
    ref.closed.subscribe((confirmed: boolean) => {
      if (confirmed !== true) return
      this.folderService.delete(folder.id).subscribe({
        next: (res) => {
          if (!res?.isSuccess) {
            this.showError('Lỗi xóa', res?.message ?? 'Không xóa được thư mục.')
            return
          }
          this.loadContents()
          this.userStorageService.refresh()
        },
        error: (err) => this.showError('Lỗi xóa', err?.error?.message ?? 'Lỗi kết nối máy chủ.'),
      })
    })
  }

  deleteFile(file: FileItem): void {
    const ref = this.ngbModal.open(ConfirmDialogModalComponent, {
      backdrop: 'static',
      centered: true,
    })
    const dlg = ref.componentInstance
    dlg.title = 'Xóa tệp'
    dlg.message = `Xóa tệp "${file.name}"? Hành động này không thể hoàn tác.`
    dlg.confirmLabel = 'Xóa'
    dlg.danger = true
    ref.closed.subscribe((confirmed: boolean) => {
      if (confirmed !== true) return
      this.fileService.delete(file.id).subscribe({
        next: (res) => {
          if (!res?.isSuccess) {
            this.showError('Lỗi xóa', res?.message ?? 'Không xóa được tệp.')
            return
          }
          this.loadContents()
          this.userStorageService.refresh()
        },
        error: (err) => this.showError('Lỗi xóa', err?.error?.message ?? 'Lỗi kết nối máy chủ.'),
      })
    })
  }

  // --- Download ---

  downloadFile(file: FileItem): void {
    this.fileService.download(file.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = file.name
        document.body.appendChild(a)
        a.click()
        document.body.removeChild(a)
        window.URL.revokeObjectURL(url)
      },
      error: (err) => this.showError('Lỗi tải xuống', err?.error?.message ?? 'Không tải xuống được tệp.'),
    })
  }

  // --- File Icon Helper ---

  getFileIcon(fileName: string): FileIconInfo {
    const ext = this.getExtension(fileName)
    return EXT_ICON_MAP[ext] ?? DEFAULT_ICON
  }

  private getExtension(fileName: string): string {
    const parts = fileName.split('.')
    if (parts.length < 2) return ''
    return parts[parts.length - 1].toLowerCase()
  }

  // --- Error ---

  private showError(title: string, message: string): void {
    showErrorModal(this.ngbModal, title, message)
  }
}
