import { CommonModule } from '@angular/common'
import { Component, Input, Output, EventEmitter } from '@angular/core'

@Component({
  selector: 'page-title',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="page-title-head d-flex align-items-center gap-2">
    @if (showBack) {
      <button
        type="button"
        class="btn btn-light btn-icon me-2"
        (click)="goBack.emit()"
        title="Quay lại"
      >
        <i class="ti ti-arrow-left"></i>
      </button>
    }
    <div [class.flex-grow-1]="showBreadcrumb" class="min-w-0">
      <h4 class="fs-16 text-uppercase fw-bold mb-0">{{ title }}</h4>
      @if (subtitle && !showBreadcrumb) {
        <p class="text-muted small mb-0 mt-1">{{ subtitle }}</p>
      }
    </div>

    @if (showBreadcrumb) {
      <div class="text-end">
        <ol class="breadcrumb m-0 py-0 fs-13">
          <li class="breadcrumb-item">
            <a href="javascript: void(0);">Cloud</a>
          </li>
          @if (subtitle) {
            <li class="breadcrumb-item">
              <a href="javascript: void(0);">{{ subtitle }}</a>
            </li>
          }
          <li class="breadcrumb-item active">{{ title }}</li>
        </ol>
      </div>
    }
  </div>`,
})
export class PageTitle {
  @Input() title: string = ''
  @Input() subtitle: string = ''
  @Input() showBack: boolean = false
  /** false: chỉ tiêu đề (và dòng phụ) bên trái — dùng cho mobile / layout gọn */
  @Input() showBreadcrumb: boolean = true
  @Output() goBack = new EventEmitter<void>()
}
