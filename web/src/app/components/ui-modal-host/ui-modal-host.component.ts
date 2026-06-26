import { UiModal } from '@/app/models/ui-modal.interface'
import { UiModalService } from '@/app/shared/ui-modal.service'
import { CommonModule, NgComponentOutlet } from '@angular/common'
import { Component, inject } from '@angular/core'
import { toSignal } from '@angular/core/rxjs-interop'

@Component({
  selector: 'app-ui-modal-host',
  standalone: true,
  imports: [CommonModule, NgComponentOutlet],
  templateUrl: './ui-modal-host.component.html',
  styles: `
    :host {
      display: contents;
    }
  `,
})
export class UiModalHostComponent {
  private uiModal = inject(UiModalService)

  protected readonly state = toSignal(this.uiModal.shell$, {
    initialValue: { visible: false, modal: null as UiModal | null },
  })

  dialogClasses(m: UiModal): Record<string, boolean> {
    const size = m.size ?? 'md'
    return {
      'modal-dialog': true,
      'modal-sm': size === 'sm',
      'modal-lg': size === 'lg',
      'modal-xl': size === 'xl',
      'modal-fullscreen': size === 'fullscreen',
      'modal-dialog-scrollable': !!m.scrollable,
    }
  }

  onBackdropClick(event: MouseEvent, m: UiModal): void {
    if (m.backdrop === 'static') return
    if (event.target !== event.currentTarget) return
    this.uiModal.closeModal()
  }

  close(): void {
    this.uiModal.closeModal()
  }

  /** Tránh biểu thức phức tạp trong microsyntax `ngComponentOutlet`. */
  componentInputsOf(m: UiModal): Record<string, unknown> {
    return m.componentInputs ?? {}
  }
}
