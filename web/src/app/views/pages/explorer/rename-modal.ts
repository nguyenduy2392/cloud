import { CommonModule } from '@angular/common'
import { Component, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap'

@Component({
  selector: 'app-rename-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">Đổi tên</h5>
      <button type="button" class="btn-close" (click)="activeModal.dismiss()"></button>
    </div>
    <div class="modal-body">
      <div class="mb-0">
        <label class="form-label" for="newName">Tên mới</label>
        <input
          type="text"
          class="form-control"
          id="newName"
          [(ngModel)]="newName"
          (keyup.enter)="onSubmit()"
          autofocus
        />
      </div>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-light" (click)="activeModal.dismiss()">Hủy</button>
      <button
        type="button"
        class="btn btn-primary"
        [disabled]="!newName.trim() || newName.trim() === currentName"
        (click)="onSubmit()"
      >
        <i class="ti ti-pencil me-1"></i>Đổi tên
      </button>
    </div>
  `,
})
export class RenameModalComponent {
  readonly activeModal = inject(NgbActiveModal)

  /** Set by the opener via componentInstance. */
  currentName = ''
  newName = ''

  onSubmit(): void {
    const name = this.newName.trim()
    if (name && name !== this.currentName) {
      this.activeModal.close(name)
    }
  }
}
