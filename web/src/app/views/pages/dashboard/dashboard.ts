import { CommonModule, DatePipe } from '@angular/common'
import { Component, OnInit } from '@angular/core'
import { RouterModule } from '@angular/router'
import { UserStorageService, type UserStorage } from '@/app/services/user-storage.service'
import { FileSizePipe, formatFileSize } from '@/app/shared/file-size.pipe'

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe, FileSizePipe],
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  storage: UserStorage | null = null
  storageLoading = true

  constructor(private userStorageService: UserStorageService) {}

  ngOnInit(): void {
    this.userStorageService.storage$.subscribe({
      next: (res) => {
        this.storageLoading = false
        if (res?.isSuccess) {
          this.storage = res.data
        }
      },
      error: () => {
        this.storageLoading = false
      },
    })
  }

  get usagePercent(): number {
    if (!this.storage || this.storage.maxBytes === 0) return 0
    return Math.min(100, (this.storage.usedBytes / this.storage.maxBytes) * 100)
  }

  get usageLabel(): string {
    if (!this.storage) return ''
    return `${formatFileSize(this.storage.usedBytes)} / ${formatFileSize(this.storage.maxBytes)}`
  }

  get progressBarClass(): string {
    const pct = this.usagePercent
    if (pct >= 90) return 'bg-danger'
    if (pct >= 70) return 'bg-warning'
    return 'bg-primary'
  }
}
