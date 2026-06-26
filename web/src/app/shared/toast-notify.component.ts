import { CUSTOM_ELEMENTS_SCHEMA, Component, inject } from '@angular/core'
import { CommonModule } from '@angular/common'
import { ToastNotifyService } from './toast-notify.service'

@Component({
  selector: 'app-toast-notify',
  standalone: true,
  imports: [CommonModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    @for (toast of toastService.getMessages(); track $index) {
      <div
        class="toast show mb-2 align-items-center text-white border-0"
        [class.bg-success]="toast.type === 'success'"
        [class.bg-warning]="toast.type === 'warning'"
        [class.bg-danger]="toast.type === 'danger'"
        role="alert"
        aria-live="assertive"
        aria-atomic="true"
        (click)="toastService.remove($index)"
        style="cursor: pointer; min-width: 280px;"
      >
        <div class="d-flex">
          <div class="toast-body py-2 px-3">
            <div class="fw-semibold">{{ toast.title }}</div>
            <div class="small opacity-75">{{ toast.content }}</div>
          </div>
          <button
            type="button"
            class="btn-close btn-close-white me-2 m-auto"
            aria-label="Close"
          ></button>
        </div>
      </div>
    }
  `,
  host: {
    class: 'toast-container position-fixed bottom-0 start-0 p-3',
    style: 'z-index: 1200',
  },
})
export class ToastNotifyComponent {
  toastService = inject(ToastNotifyService)
}
