import { CommonModule } from '@angular/common'
import { Component, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap'

@Component({
  selector: 'app-create-folder-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">Tạo thư mục mới</h5>
      <button type="button" class="btn-close" (click)="activeModal.dismiss()"></button>
    </div>
    <div class="modal-body">
      <div class="mb-0">
        <label class="form-label" for="folderName">Tên thư mục</label>
        <input
          type="text"
          class="form-control"
          id="folderName"
          [(ngModel)]="folderName"
          (keyup.enter)="onSubmit()"
          placeholder="Nhập tên thư mục..."
          autofocus
        />
      </div>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-light" (click)="activeModal.dismiss()">Hủy</button>
      <button
        type="button"
        class="btn btn-primary"
        [disabled]="!folderName.trim()"
        (click)="onSubmit()"
      >
        <i class="ti ti-folder-plus me-1"></i>Tạo
      </button>
    </div>
  `,
})
export class CreateFolderModalComponent {
  readonly activeModal = inject(NgbActiveModal)
  folderName = ''

  onSubmit(): void {
    const name = this.folderName.trim()
    if (name) {
      this.activeModal.close(name)
    }
  }
}
