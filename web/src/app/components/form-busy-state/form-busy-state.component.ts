import { CommonModule } from '@angular/common'
import { Component, Input } from '@angular/core'

/**
 * Trạng thái chờ khi form đang tải dữ liệu (modal, drawer, trang…).
 * Dùng chung: truyền `title` và `subtitle`.
 */
@Component({
  selector: 'app-form-busy-state',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './form-busy-state.component.html',
  styleUrl: './form-busy-state.component.scss',
})
export class FormBusyStateComponent {
  /** Tiêu đề chính (in đậm). */
  @Input() title = 'Đang chuẩn bị form'

  /** Dòng mô tả phía dưới. */
  @Input() subtitle = ''

  /** Class icon Tabler trong lõi (vd. `ti ti-chart-bar`). */
  @Input() iconClass = 'ti ti-chart-bar'

  /** Quay vòng progress ngoài. */
  @Input() spinRing = true
}
