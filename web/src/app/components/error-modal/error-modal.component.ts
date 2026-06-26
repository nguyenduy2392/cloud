import { Component, Input, inject } from '@angular/core'
import { CommonModule } from '@angular/common'
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap'

/**
 * Modal lỗi dùng chung (NgbModal) — layout căn giữa, nút xanh như mockup.
 * Gán `title`, `message`, `confirmLabel`, `htmlMessage` qua `ref.componentInstance` sau `modal.open(...)`.
 */
@Component({
  selector: 'app-error-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './error-modal.component.html',
  styleUrl: './error-modal.component.scss',
})
export class ErrorModalComponent {
  readonly activeModal = inject(NgbActiveModal)

  /** Tiêu đề lớn (ví dụ "Oops!"). */
  @Input() title = 'Oops!'

  /** Mô tả lỗi. */
  @Input() message = ''

  /** Nhãn nút hành động chính (Hỗ trợ). */
  @Input() confirmLabel = 'Hỗ trợ'

  /** Nhãn nút phụ (Đóng). */
  @Input() cancelLabel = 'Đóng'

  /**
   * Nếu `true`, `message` render HTML (`innerHTML`).
   * Chỉ dùng với nội dung tin cậy.
   */
  @Input() htmlMessage = false
}
