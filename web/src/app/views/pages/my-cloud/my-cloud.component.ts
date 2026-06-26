import { CommonModule, DatePipe } from '@angular/common'
import { Component, OnInit, OnDestroy, HostListener, inject, effect } from '@angular/core'
import { ActivatedRoute, Router, RouterModule } from '@angular/router'
import { FormsModule } from '@angular/forms'
import {
  NgbDropdownModule,
  NgbModal,
  NgbModalModule,
} from '@ng-bootstrap/ng-bootstrap'
import { Subject, takeUntil, combineLatest } from 'rxjs'
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
import { EnvService } from '@/app/services/env.service'
import { SearchService } from '@/app/services/search.service'
import { UserStorageService } from '@/app/services/user-storage.service'
import {
  UploadWorkerService,
  type UploadTask,
} from '@/app/services/upload-worker.service'
import { DomSanitizer } from '@angular/platform-browser'
import { CreateFolderModalComponent } from '../explorer/create-folder-modal'
import { RenameModalComponent } from '../explorer/rename-modal'
import {
  FilePreviewModalComponent,
  getPreviewType,
} from '@/app/components/file-preview-modal/file-preview-modal.component'
import { ShareModalComponent } from '@/app/components/share-modal/share-modal.component'
import { ResourcePermissionService } from '@/app/services/resource-permission.service'

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

const PAGE_SIZE = 40

type ViewMode = 'list' | 'grid'

@Component({
  selector: 'app-my-cloud',
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
  templateUrl: './my-cloud.component.html',
  styles: `
    :host { display: block; }
    .folder-row { cursor: pointer; }
    .folder-row:hover { background-color: rgba(0,0,0,.03); }
    .file-icon { font-size: 1.25rem; }
    .folder-icon { font-size: 1.25rem; color: #f0ad4e; }
    .my-cloud-toolbar {
      position: sticky;
      top: 50px;
      z-index: 100;
      background: var(--greeva-body-bg);
      padding-top: 0.75rem;
      padding-bottom: 0.75rem;
      margin-bottom: 0 !important;
    }
    ::ng-deep .my-cloud-thead th {
      position: sticky;
      top: 99px;
      z-index: 99;
      background: var(--greeva-tertiary-bg);
    }
    .my-cloud-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 12px;
      padding: 16px;
    }
    @media (min-width: 576px) {
      .my-cloud-grid { grid-template-columns: repeat(3, 1fr); }
    }
    @media (min-width: 992px) {
      .my-cloud-grid { grid-template-columns: repeat(4, 1fr); }
    }
    @media (min-width: 1400px) {
      .my-cloud-grid { grid-template-columns: repeat(5, 1fr); }
    }
    .grid-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      border-radius: 8px;
      cursor: pointer;
      text-align: center;
      transition: background 0.15s;
      position: relative;
      overflow: hidden;
      border: 1px solid var(--greeva-border-color-translucent);
    }
    .grid-item:hover {
      background: var(--greeva-tertiary-bg);
    }
    .grid-item__preview {
      width: 100%;
      aspect-ratio: 4 / 3;
      overflow: hidden;
      background: var(--greeva-tertiary-bg);
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .grid-item__preview img,
    .grid-item__preview video {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    .grid-item__icon-wrap {
      width: 100%;
      aspect-ratio: 4 / 3;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--greeva-tertiary-bg);
    }
    .grid-item__icon {
      font-size: 3rem;
    }
    .grid-item__body {
      width: 100%;
      padding: 8px 10px;
    }
    .grid-item__name {
      font-size: 0.8125rem;
      font-weight: 500;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      max-width: 100%;
    }
    .grid-item__meta {
      font-size: 0.7rem;
    }
    .grid-item__actions {
      position: absolute;
      top: 4px;
      right: 4px;
      opacity: 0;
      transition: opacity 0.15s;
    }
    .grid-item:hover .grid-item__actions {
      opacity: 1;
    }
    .view-toggle .btn.active {
      background: var(--greeva-primary);
      color: #fff;
      border-color: var(--greeva-primary);
    }
  `,
})
export class MyCloudComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>()

  loading = false
  loadingMore = false
  mode: 'cloud' | 'shared' = 'cloud'
  currentFolderId?: string
  breadcrumbs: FolderItem[] = []
  folders: FolderItem[] = []
  files: FileItem[] = []
  viewMode: ViewMode = (localStorage.getItem('my-cloud-view') as ViewMode) || 'list'

  private page = 1
  private totalFolders = 0
  private totalFiles = 0

  private searchService = inject(SearchService)
  private searchTerm = ''
  private searchDebounce: any

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private env: EnvService,
    private sanitizer: DomSanitizer,
    private folderService: FolderService,
    private fileService: CloudFileService,
    private userStorageService: UserStorageService,
    private uploadWorker: UploadWorkerService,
    private resourcePermissionService: ResourcePermissionService,
    private ngbModal: NgbModal
  ) {
    effect(() => {
      const term = this.searchService.term()
      clearTimeout(this.searchDebounce)
      this.searchDebounce = setTimeout(() => {
        if (term !== this.searchTerm) {
          this.searchTerm = term
          this.loadFirst()
        }
      }, 400)
    })
  }

  get isRoot(): boolean {
    return !this.currentFolderId
  }

  private seenUploadIds = new Set<string>()

  ngOnInit(): void {
    combineLatest([this.route.data, this.route.paramMap, this.route.queryParamMap])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([data, params, qp]) => {
        this.mode = data['mode'] === 'shared' || qp.get('from') === 'shared'
          ? 'shared' : 'cloud'
        this.currentFolderId = params.get('id') ?? undefined
        this.searchService.clear()
        this.searchTerm = ''
        this.loadFirst()
      })


    this.uploadWorker.tasks$
      .pipe(takeUntil(this.destroy$))
      .subscribe((tasks) => {
        for (const task of tasks) {
          if (
            task.status === 'done' &&
            task.uploadedFile &&
            !this.seenUploadIds.has(task.id)
          ) {
            this.seenUploadIds.add(task.id)
            const uploaded = task.uploadedFile
            const uploadedFolderId = uploaded.folderId ?? undefined
            if (uploadedFolderId === this.currentFolderId) {
              this.insertFile(uploaded)
            }
          }
        }
      })
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  @HostListener('window:scroll')
  onScroll(): void {
    if (this.loading || this.loadingMore) return
    if (!this.hasMore) return

    const threshold = 200
    const position = window.innerHeight + window.scrollY
    const height = document.documentElement.scrollHeight

    if (position >= height - threshold) {
      this.loadMore()
    }
  }

  setViewMode(mode: ViewMode): void {
    this.viewMode = mode
    localStorage.setItem('my-cloud-view', mode)
  }

  get isEmpty(): boolean {
    return this.folders.length === 0 && this.files.length === 0
  }

  get hasMoreFolders(): boolean {
    return this.folders.length < this.totalFolders
  }

  get hasMoreFiles(): boolean {
    return this.files.length < this.totalFiles
  }

  get hasMore(): boolean {
    return this.hasMoreFolders || this.hasMoreFiles
  }

  loadFirst(): void {
    this.loading = true
    this.page = 1
    this.folders = []
    this.files = []
    this.fetchPage(1, true)
  }

  loadMore(): void {
    if (!this.hasMore || this.loadingMore) return
    this.loadingMore = true
    this.page++
    this.fetchPage(this.page, false)
  }

  private fetchPage(page: number, isFirst: boolean): void {
    const keyword = this.searchTerm ? this.removeDiacritics(this.searchTerm) : undefined
    const useSharedApi = this.mode === 'shared' && !this.currentFolderId
    const request$ = useSharedApi
      ? this.resourcePermissionService.getSharedWithMe(page, PAGE_SIZE, keyword)
      : this.folderService.getContents(this.currentFolderId, page, PAGE_SIZE, keyword)
    request$.subscribe({
      next: (res) => {
        this.loading = false
        this.loadingMore = false
        if (!res?.isSuccess) {
          if (isFirst) {
            this.showError('Lỗi tải dữ liệu', res?.message ?? 'Không tải được nội dung.')
          }
          return
        }
        const data: any = res.data
        if (isFirst) {
          this.breadcrumbs = data?.breadcrumbs ?? []
        }
        const newFolders: FolderItem[] = data?.folders?.items ?? []
        const newFiles: FileItem[] = data?.files?.items ?? []
        this.totalFolders = data?.folders?.totalCount ?? 0
        this.totalFiles = data?.files?.totalCount ?? 0

        this.folders = this.mergeById(this.folders, newFolders)
        this.files = this.mergeById(this.files, newFiles)
      },
      error: (err) => {
        this.loading = false
        this.loadingMore = false
        if (!isFirst) this.page--
        if (isFirst) {
          this.showError('Lỗi tải dữ liệu', err?.error?.message ?? 'Lỗi kết nối máy chủ.')
        }
      },
    })
  }

  private mergeById<T extends { id: string }>(existing: T[], incoming: T[]): T[] {
    const ids = new Set(existing.map((item) => item.id))
    const toAdd = incoming.filter((item) => !ids.has(item.id))
    if (toAdd.length === 0) return existing
    return [...existing, ...toAdd]
  }

  private insertFile(data: UploadTask['uploadedFile']): void {
    if (!data) return
    if (this.files.some((f) => f.id === data.id)) return

    const newFile: FileItem = {
      id: data.id,
      name: data.name,
      sizeInBytes: data.sizeInBytes,
      contentType: data.contentType ?? null,
      extension: data.extension ?? null,
      folderId: data.folderId ?? null,
      createdAt: data.createdAt,
      createdByName: data.createdByName ?? null,
    }

    const idx = this.files.findIndex(
      (f) => f.name.localeCompare(newFile.name) > 0
    )
    if (idx === -1) {
      this.files = [...this.files, newFile]
    } else {
      const copy = [...this.files]
      copy.splice(idx, 0, newFile)
      this.files = copy
    }
    this.totalFiles++
  }

  // --- Share ---

  shareFolder(folder: FolderItem): void {
    this.openShareModal(0, folder.id, folder.name)
  }

  shareFile(file: FileItem): void {
    this.openShareModal(1, file.id, file.name)
  }

  private openShareModal(resourceType: number, resourceId: string, resourceName: string): void {
    const ref = this.ngbModal.open(ShareModalComponent, {
      centered: true,
      backdrop: 'static',
      size: 'md',
    })
    ref.componentInstance.resourceType = resourceType
    ref.componentInstance.resourceId = resourceId
    ref.componentInstance.resourceName = resourceName
  }

  // --- Navigation ---

  openFolder(folder: FolderItem): void {
    const qp = this.mode === 'shared' ? { queryParams: { from: 'shared' } } : {}
    this.router.navigate(['/folder', folder.id], qp)
  }

  navigateBreadcrumb(crumb?: FolderItem): void {
    if (!crumb) {
      this.router.navigate([this.mode === 'shared' ? '/shared-with-me' : '/my-cloud'])
    } else {
      const qp = this.mode === 'shared' ? { queryParams: { from: 'shared' } } : {}
      this.router.navigate(['/folder', crumb.id], qp)
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
          this.loadFirst()
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
          const data: any = res.data
          folder.name = data?.Name ?? data?.name ?? newName
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
          const data: any = res.data
          file.name = data?.Name ?? data?.name ?? newName
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
          this.folders = this.folders.filter((f) => f.id !== folder.id)
          this.totalFolders = Math.max(0, this.totalFolders - 1)
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
          this.files = this.files.filter((f) => f.id !== file.id)
          this.totalFiles = Math.max(0, this.totalFiles - 1)
          this.userStorageService.refresh()
        },
        error: (err) => this.showError('Lỗi xóa', err?.error?.message ?? 'Lỗi kết nối máy chủ.'),
      })
    })
  }

  // --- Download & Preview ---

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

  openFile(file: FileItem): void {
    const type = getPreviewType(file.name)
    if (type === 'unsupported') {
      this.downloadFile(file)
      return
    }

    if (type === 'docx' || type === 'spreadsheet') {
      this.fileService.download(file.id).subscribe({
        next: (blob) => this.openPreviewModal(file, type, blob),
        error: () => this.downloadFile(file),
      })
      return
    }

    this.openPreviewModal(file, type)
  }

  private openPreviewModal(file: FileItem, type: string, blob?: Blob): void {
    const url = this.previewUrl(file.id)
    const ref = this.ngbModal.open(FilePreviewModalComponent, {
      size: 'xl',
      centered: true,
      backdrop: 'static',
      windowClass: 'file-preview-fullscreen',
    })
    ref.componentInstance.fileName = file.name
    ref.componentInstance.fileSize = file.sizeInBytes
    ref.componentInstance.type = type
    ref.componentInstance.previewUrl =
      type === 'pdf'
        ? this.sanitizer.bypassSecurityTrustResourceUrl(url)
        : url
    if (blob) ref.componentInstance.fileBlob = blob
    ref.closed.subscribe((action: string) => {
      if (action === 'download') this.downloadFile(file)
    })
  }

  // --- Preview helpers ---

  private static readonly IMAGE_EXTS = new Set(['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp', 'bmp'])
  private static readonly VIDEO_EXTS = new Set(['mp4', 'webm', 'ogg'])

  isImage(fileName: string): boolean {
    return MyCloudComponent.IMAGE_EXTS.has(this.getExtension(fileName))
  }

  isVideo(fileName: string): boolean {
    return MyCloudComponent.VIDEO_EXTS.has(this.getExtension(fileName))
  }

  previewUrl(fileId: string): string {
    const token = localStorage.getItem('token')?.trim() ?? ''
    return `${this.env.apiUrl}/api/files/${fileId}/preview?t=${token}`
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

  // --- Search helper ---

  private removeDiacritics(str: string): string {
    return str
      .normalize('NFD')
      .replace(/[̀-ͯ]/g, '')
      .replace(/đ/g, 'd')
      .replace(/Đ/g, 'D')
      .toLowerCase()
  }

  // --- Error ---

  private showError(title: string, message: string): void {
    showErrorModal(this.ngbModal, title, message)
  }
}
