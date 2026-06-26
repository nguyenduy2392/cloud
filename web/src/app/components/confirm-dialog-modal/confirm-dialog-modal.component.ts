import { CommonModule } from '@angular/common'
import { Component, inject } from '@angular/core'
import {
  NgbActiveModal,
  type NgbModalOptions,
} from '@ng-bootstrap/ng-bootstrap'

/** Tùy chọn mở modal mặc định: căn trên viewport (xem `.modal.confirm-dialog-top` trong `styles.scss`). */
export const confirmDialogModalOptions: NgbModalOptions = {
  backdrop: 'static',
  centered: false,
  windowClass: 'confirm-dialog-top',
}

/**
 * Modal xác nhận dùng chung (NgbModal).
 * Gán `title`, `message`, `confirmLabel`, `cancelLabel`, `danger` qua `ref.componentInstance` sau `modal.open(...)`.
 * Đồng ý: `activeModal.close(true)`; hủy/đóng: `dismiss()`.
 */
@Component({
  selector: 'app-confirm-dialog-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-dialog-modal.component.html',
})
export class ConfirmDialogModalComponent {
  readonly activeModal = inject(NgbActiveModal)

  /** Tiêu đề thanh header modal. */
  title = 'Xác nhận'

  /** Nội dung hỏi (thuần text). */
  message = ''

  /** Nhãn nút xác nhận. */
  confirmLabel = 'Đồng ý'

  /** Nhãn nút hủy. */
  cancelLabel = 'Hủy'

  /** `true`: nút xác nhận dùng `btn-danger` (thao tác xóa). */
  danger = false

  /**
   * Nội dung cảnh báo hiển thị dạng `alert alert-warning`.
   * Để trống/thêm nếu không cần.
   */
  warningMessage = ''
}
