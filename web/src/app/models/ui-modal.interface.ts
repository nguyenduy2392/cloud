import type { Type } from '@angular/core'

export type UiModalSize = 'sm' | 'md' | 'lg' | 'xl' | 'fullscreen'

/**
 * Cấu hình modal toàn cục — truyền vào {@link UiModalService.openModal} / {@link UiModalService.create}.
 */
export interface UiModal {
  /** Tiêu đề trên header (nút đóng luôn hiển thị). */
  title?: string

  size?: UiModalSize

  backdrop?: boolean | 'static'

  scrollable?: boolean

  /**
   * Component standalone làm nội dung phần body (dưới header).
   * Dùng kèm `componentInputs` nếu cần truyền `@Input()`.
   */
  component?: Type<unknown>

  /** Map input → giá trị cho `ngComponentOutlet`. */
  componentInputs?: Record<string, unknown>
}
