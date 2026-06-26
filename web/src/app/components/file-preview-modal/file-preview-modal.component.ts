import {
  Component,
  inject,
  Input,
  ViewChild,
  ElementRef,
  AfterViewInit,
} from '@angular/core'
import { CommonModule } from '@angular/common'
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap'
import { type SafeResourceUrl } from '@angular/platform-browser'
import { FileSizePipe } from '@/app/shared/file-size.pipe'

export type PreviewType =
  | 'image'
  | 'video'
  | 'pdf'
  | 'docx'
  | 'spreadsheet'
  | 'unsupported'

const IMAGE_EXTS = new Set(['jpg', 'jpeg', 'png', 'gif', 'svg', 'webp', 'bmp'])
const VIDEO_EXTS = new Set(['mp4', 'webm', 'ogg', 'mov'])
const PDF_EXTS = new Set(['pdf'])
const DOCX_EXTS = new Set(['docx'])
const SPREADSHEET_EXTS = new Set(['xlsx', 'xls', 'csv'])

export function getPreviewType(fileName: string): PreviewType {
  const ext = fileName.split('.').pop()?.toLowerCase() ?? ''
  if (IMAGE_EXTS.has(ext)) return 'image'
  if (VIDEO_EXTS.has(ext)) return 'video'
  if (PDF_EXTS.has(ext)) return 'pdf'
  if (DOCX_EXTS.has(ext)) return 'docx'
  if (SPREADSHEET_EXTS.has(ext)) return 'spreadsheet'
  return 'unsupported'
}

@Component({
  selector: 'app-file-preview-modal',
  standalone: true,
  imports: [CommonModule, FileSizePipe],
  template: `
    <div class="preview-modal">
      <div class="preview-modal__header">
        <span class="preview-modal__title" [title]="fileName">{{ fileName }}</span>
        <span class="preview-modal__size text-muted">{{ fileSize | fileSize }}</span>
        <div class="preview-modal__actions">
          <button class="preview-modal__btn" (click)="onDownload()" title="Tải xuống">
            <i class="ti ti-download"></i>
          </button>
          <button class="preview-modal__btn" (click)="activeModal.dismiss()" title="Đóng">
            <i class="ti ti-x"></i>
          </button>
        </div>
      </div>
      <div class="preview-modal__body" [class.preview-modal__body--doc]="type === 'docx' || type === 'spreadsheet'">
        @if (docLoading) {
          <div class="preview-modal__loading">
            <div class="spinner-border text-primary" role="status"></div>
            <span class="ms-2">Đang tải...</span>
          </div>
        }
        @if (docError) {
          <div class="preview-modal__error text-danger">
            <i class="ti ti-alert-circle me-2"></i>{{ docError }}
          </div>
        }
        @switch (type) {
          @case ('image') {
            <img [src]="previewUrl" [alt]="fileName" class="preview-modal__image" />
          }
          @case ('video') {
            <video [src]="previewUrl" controls autoplay class="preview-modal__video"></video>
          }
          @case ('pdf') {
            <iframe [src]="previewUrl" class="preview-modal__pdf" frameborder="0"></iframe>
          }
          @case ('docx') {
            <div #docxContainer class="preview-modal__docx"></div>
          }
          @case ('spreadsheet') {
            <div class="preview-modal__spreadsheet" [innerHTML]="sheetHtml"></div>
          }
        }
      </div>
    </div>
  `,
  styles: `
    .preview-modal {
      display: flex;
      flex-direction: column;
      height: 90vh;
    }
    .preview-modal__header {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      border-bottom: 1px solid var(--greeva-border-color);
      flex-shrink: 0;
    }
    .preview-modal__title {
      font-weight: 600;
      font-size: 0.9375rem;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      flex: 1;
    }
    .preview-modal__size {
      font-size: 0.8125rem;
      white-space: nowrap;
    }
    .preview-modal__actions {
      display: flex;
      gap: 4px;
      margin-left: auto;
    }
    .preview-modal__btn {
      background: none;
      border: none;
      padding: 4px 8px;
      cursor: pointer;
      color: var(--greeva-secondary-color);
      border-radius: 4px;
      font-size: 1.125rem;
      line-height: 1;
    }
    .preview-modal__btn:hover {
      background: var(--greeva-secondary-bg);
      color: var(--greeva-body-color);
    }
    .preview-modal__body {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      overflow: hidden;
      background: #000;
    }
    .preview-modal__body--doc {
      background: var(--greeva-body-bg);
      overflow: auto;
      align-items: flex-start;
      justify-content: flex-start;
    }
    .preview-modal__image {
      max-width: 100%;
      max-height: 100%;
      object-fit: contain;
    }
    .preview-modal__video {
      max-width: 100%;
      max-height: 100%;
    }
    .preview-modal__pdf {
      width: 100%;
      height: 100%;
      border: none;
    }
    .preview-modal__docx {
      width: 100%;
      padding: 16px;
      overflow: auto;
    }
    .preview-modal__spreadsheet {
      width: 100%;
      padding: 16px;
      overflow: auto;
    }
    ::ng-deep .preview-modal__spreadsheet table {
      border-collapse: collapse;
      width: 100%;
      font-size: 0.8125rem;
    }
    ::ng-deep .preview-modal__spreadsheet td,
    ::ng-deep .preview-modal__spreadsheet th {
      border: 1px solid var(--greeva-border-color);
      padding: 4px 8px;
      white-space: nowrap;
    }
    ::ng-deep .preview-modal__spreadsheet th {
      background: var(--greeva-tertiary-bg);
      font-weight: 600;
    }
    .preview-modal__loading {
      display: flex;
      align-items: center;
      padding: 32px;
      color: var(--greeva-body-color);
    }
    .preview-modal__error {
      padding: 32px;
      font-size: 0.875rem;
    }
  `,
})
export class FilePreviewModalComponent implements AfterViewInit {
  readonly activeModal = inject(NgbActiveModal)

  @Input() fileName = ''
  @Input() fileSize = 0
  @Input() previewUrl: string | SafeResourceUrl = ''
  @Input() type: PreviewType = 'image'
  @Input() fileBlob?: Blob

  @ViewChild('docxContainer') docxContainer?: ElementRef<HTMLDivElement>

  docLoading = false
  docError = ''
  sheetHtml = ''

  ngAfterViewInit(): void {
    if (this.type === 'docx' && this.fileBlob) {
      this.renderDocx(this.fileBlob)
    }
    if (this.type === 'spreadsheet' && this.fileBlob) {
      this.renderSpreadsheet(this.fileBlob)
    }
  }

  onDownload(): void {
    this.activeModal.close('download')
  }

  private async renderDocx(blob: Blob): Promise<void> {
    this.docLoading = true
    try {
      const docxPreview = await import('docx-preview')
      const arrayBuffer = await blob.arrayBuffer()
      await docxPreview.renderAsync(arrayBuffer, this.docxContainer!.nativeElement, undefined, {
        className: 'docx-preview-wrapper',
        inWrapper: true,
        ignoreWidth: false,
        ignoreHeight: true,
      })
      this.docLoading = false
    } catch (e) {
      this.docLoading = false
      this.docError = 'Không thể hiển thị tệp này.'
    }
  }

  private async renderSpreadsheet(blob: Blob): Promise<void> {
    this.docLoading = true
    try {
      const XLSX = await import('xlsx')
      const arrayBuffer = await blob.arrayBuffer()
      const workbook = XLSX.read(arrayBuffer, { type: 'array' })
      const firstSheet = workbook.Sheets[workbook.SheetNames[0]]
      this.sheetHtml = XLSX.utils.sheet_to_html(firstSheet, { editable: false })
      this.docLoading = false
    } catch (e) {
      this.docLoading = false
      this.docError = 'Không thể hiển thị tệp này.'
    }
  }
}
