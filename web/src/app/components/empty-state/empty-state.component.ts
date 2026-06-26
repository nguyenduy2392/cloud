import { CommonModule } from '@angular/common'
import { Component, EventEmitter, Input, Output } from '@angular/core'

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="d-flex flex-column align-items-center justify-content-center py-5 px-4 text-center">
      @if (icon) {
        <div class="text-muted mb-3 fs-1">
          <i class="{{ icon }}"></i>
        </div>
      }
      @if (title) {
        <h5 class="fw-semibold text-body mb-2">{{ title }}</h5>
      }
      @if (description) {
        <p class="text-muted mb-4 small mx-auto" style="max-width: 380px">{{ description }}</p>
      }
      @if (buttonLabel) {
        <button
          type="button"
          class="btn btn-primary"
          (click)="buttonClick.emit()"
          [disabled]="buttonDisabled"
        >
          <i class="ti ti-plus me-1"></i>{{ buttonLabel }}
        </button>
      }
    </div>
  `,
  styles: `
    :host { display: block; }
  `,
})
export class AppEmptyStateComponent {
  @Input() icon = 'ti ti-inbox'
  @Input() title = ''
  @Input() description = ''
  @Input() buttonLabel = ''
  @Input() buttonDisabled = false
  @Output() buttonClick = new EventEmitter<void>()
}