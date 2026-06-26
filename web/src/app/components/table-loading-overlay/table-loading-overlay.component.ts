import { CommonModule } from '@angular/common'
import { Component, Input } from '@angular/core'

/**
 * Overlay phủ lên vùng bảng (container cần `position: relative`) để hiển thị loading,
 * căn giữa theo khung nhìn thấy, không phụ thuộc scroll ngang của bảng.
 */
@Component({
  selector: 'app-table-loading-overlay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './table-loading-overlay.component.html',
  styles: `
    .table-loading-inner {
      background: transparent;
    }

    .table-loading-overlay {
      position: absolute;
      inset: 0;
      z-index: 20;
      display: flex;
      align-items: center;
      justify-content: center;
      pointer-events: none;
      background: rgba(255, 255, 255, 0.55);
      backdrop-filter: blur(2px);
    }
  `,
})
export class TableLoadingOverlayComponent {
  /** Tiêu đề ngắn (ví dụ: "Đang tải dữ liệu"). */
  @Input() title = 'Đang tải dữ liệu'

  /** Dòng mô tả phụ; để trống để ẩn. */
  @Input() subtitle = 'Vui lòng chờ trong giây lát…'

  /** Màu spinner Bootstrap: primary | secondary | success | danger | warning | info | light | dark */
  @Input() spinnerVariant:
    | 'primary'
    | 'secondary'
    | 'success'
    | 'danger'
    | 'warning'
    | 'info'
    | 'light'
    | 'dark' = 'primary'
}
