import { Component, HostListener, inject, OnInit, OnDestroy } from '@angular/core'
import { CommonModule } from '@angular/common'
import { Subject, takeUntil } from 'rxjs'
import {
  UploadWorkerService,
  type UploadTask,
} from '@/app/services/upload-worker.service'
import { FileSizePipe } from '@/app/shared/file-size.pipe'

@Component({
  selector: 'app-upload-worker',
  standalone: true,
  imports: [CommonModule, FileSizePipe],
  templateUrl: './upload-worker.component.html',
  styleUrl: './upload-worker.component.scss',
})
export class UploadWorkerComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>()
  private uploadWorker = inject(UploadWorkerService)

  tasks: UploadTask[] = []
  collapsed = false

  ngOnInit(): void {
    this.uploadWorker.tasks$
      .pipe(takeUntil(this.destroy$))
      .subscribe((tasks) => {
        this.tasks = tasks
        if (tasks.length > 0 && this.collapsed) {
          const hasNew = tasks.some((t) => t.status === 'pending')
          if (hasNew) this.collapsed = false
        }
      })
  }

  ngOnDestroy(): void {
    this.destroy$.next()
    this.destroy$.complete()
  }

  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.uploadWorker.hasActiveUploads) {
      event.preventDefault()
    }
  }

  get visible(): boolean {
    return this.tasks.length > 0
  }

  get activeTasks(): UploadTask[] {
    return this.tasks.filter(
      (t) => t.status === 'uploading' || t.status === 'pending'
    )
  }

  get completedCount(): number {
    return this.tasks.filter((t) => t.status === 'done').length
  }

  get totalCount(): number {
    return this.tasks.length
  }

  get overallProgress(): number {
    const active = this.tasks.filter(
      (t) => t.status !== 'cancelled'
    )
    if (active.length === 0) return 100
    const total = active.reduce((sum, t) => sum + t.progress, 0)
    return Math.round(total / active.length)
  }

  formatEta(seconds?: number): string {
    if (seconds == null || seconds <= 0) return ''
    if (seconds < 60) return `${seconds}s`
    const m = Math.floor(seconds / 60)
    const s = seconds % 60
    return `${m}m ${s}s`
  }

  toggleCollapse(): void {
    this.collapsed = !this.collapsed
  }

  cancel(task: UploadTask): void {
    this.uploadWorker.cancel(task.id)
  }

  dismiss(task: UploadTask): void {
    this.uploadWorker.dismiss(task.id)
  }

  retry(task: UploadTask): void {
    this.uploadWorker.retry(task.id)
  }

  dismissAll(): void {
    this.uploadWorker.dismissAll()
  }

  statusIcon(task: UploadTask): string {
    switch (task.status) {
      case 'done':
        return 'ti-circle-check-filled text-success'
      case 'error':
        return 'ti-alert-circle-filled text-danger'
      case 'cancelled':
        return 'ti-circle-minus text-muted'
      default:
        return 'ti-circle-arrow-up text-primary'
    }
  }

  trackById(_: number, task: UploadTask): string {
    return task.id
  }
}
