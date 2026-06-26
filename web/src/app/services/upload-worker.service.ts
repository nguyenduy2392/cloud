import { Injectable } from '@angular/core'
import { BehaviorSubject, firstValueFrom } from 'rxjs'
import { EnvService } from './env.service'
import { UserStorageService, type UserStorage } from './user-storage.service'

export interface UploadedFileData {
  id: string
  name: string
  sizeInBytes: number
  contentType?: string
  extension?: string
  folderId?: string | null
  createdAt: string
  createdByName?: string
}

export interface UploadTask {
  id: string
  file: File
  folderId?: string
  progress: number
  status: 'pending' | 'uploading' | 'done' | 'error' | 'cancelled'
  error?: string
  startedAt?: number
  estimatedSecondsLeft?: number
  xhr?: XMLHttpRequest
  uploadedFile?: UploadedFileData
}

@Injectable({ providedIn: 'root' })
export class UploadWorkerService {
  private tasksSubject = new BehaviorSubject<UploadTask[]>([])
  tasks$ = this.tasksSubject.asObservable()

  private idCounter = 0

  constructor(
    private env: EnvService,
    private userStorageService: UserStorageService
  ) {}

  get hasActiveUploads(): boolean {
    return this.tasksSubject.value.some(
      (t) => t.status === 'uploading' || t.status === 'pending'
    )
  }

  async enqueue(file: File, folderId?: string): Promise<void> {
    const quotaError = await this.checkQuota(file.size)
    const task: UploadTask = {
      id: `upload-${++this.idCounter}-${Date.now()}`,
      file,
      folderId,
      progress: 0,
      status: quotaError ? 'error' : 'pending',
      error: quotaError ?? undefined,
    }
    this.updateTasks([...this.tasksSubject.value, task])
    if (!quotaError) this.processNext()
  }

  private async checkQuota(fileSize: number): Promise<string | null> {
    try {
      const res = await firstValueFrom(this.userStorageService.storage$)
      if (!res?.isSuccess || !res.data) return null
      const { usedBytes, maxBytes } = res.data
      if (maxBytes <= 0) return null

      const pendingBytes = this.tasksSubject.value
        .filter((t) => t.status === 'pending' || t.status === 'uploading')
        .reduce((sum, t) => sum + t.file.size, 0)

      if (usedBytes + pendingBytes + fileSize > maxBytes) {
        const remaining = Math.max(0, maxBytes - usedBytes - pendingBytes)
        return `Dung lượng không đủ. Còn trống ${this.formatBytes(remaining)}, tệp cần ${this.formatBytes(fileSize)}`
      }
      return null
    } catch {
      return null
    }
  }

  retry(taskId: string): void {
    const tasks = this.tasksSubject.value
    const task = tasks.find((t) => t.id === taskId)
    if (!task || (task.status !== 'error' && task.status !== 'cancelled')) return

    task.status = 'pending'
    task.progress = 0
    task.error = undefined
    task.estimatedSecondsLeft = undefined
    task.startedAt = undefined
    this.updateTasks([...tasks])
    this.processNext()
  }

  cancel(taskId: string): void {
    const tasks = this.tasksSubject.value
    const task = tasks.find((t) => t.id === taskId)
    if (!task) return

    if (task.xhr) {
      task.xhr.abort()
    }
    task.status = 'cancelled'
    task.xhr = undefined
    this.updateTasks([...tasks])
    this.processNext()
  }

  dismiss(taskId: string): void {
    this.updateTasks(this.tasksSubject.value.filter((t) => t.id !== taskId))
  }

  dismissAll(): void {
    this.updateTasks(
      this.tasksSubject.value.filter(
        (t) => t.status === 'uploading' || t.status === 'pending'
      )
    )
  }

  private processNext(): void {
    const tasks = this.tasksSubject.value
    const activeCount = tasks.filter((t) => t.status === 'uploading').length
    if (activeCount >= 2) return

    const next = tasks.find((t) => t.status === 'pending')
    if (!next) return

    this.startUpload(next)
  }

  private startUpload(task: UploadTask): void {
    task.status = 'uploading'
    task.startedAt = Date.now()
    this.updateTasks([...this.tasksSubject.value])

    const formData = new FormData()
    formData.append('file', task.file)
    if (task.folderId) formData.append('folderId', task.folderId)

    const xhr = new XMLHttpRequest()
    task.xhr = xhr

    xhr.upload.addEventListener('progress', (e) => {
      if (!e.lengthComputable) return
      task.progress = Math.round((e.loaded / e.total) * 100)

      const elapsed = (Date.now() - (task.startedAt ?? Date.now())) / 1000
      if (elapsed > 0 && task.progress > 0) {
        const totalEstimated = elapsed / (task.progress / 100)
        task.estimatedSecondsLeft = Math.max(
          0,
          Math.round(totalEstimated - elapsed)
        )
      }

      this.updateTasks([...this.tasksSubject.value])
    })

    xhr.addEventListener('load', () => {
      task.xhr = undefined
      if (xhr.status >= 200 && xhr.status < 300) {
        const apiError = this.parseApiFailure(xhr)
        if (apiError) {
          task.status = 'error'
          task.error = apiError
        } else {
          task.status = 'done'
          task.progress = 100
          task.estimatedSecondsLeft = 0
          task.uploadedFile = this.parseUploadedFile(xhr)
          this.userStorageService.refresh()
        }
      } else {
        task.status = 'error'
        task.error = this.parseXhrError(xhr)
      }
      this.updateTasks([...this.tasksSubject.value])
      this.processNext()
    })

    xhr.addEventListener('error', () => {
      task.xhr = undefined
      task.status = 'error'
      task.error = 'Lỗi kết nối'
      this.updateTasks([...this.tasksSubject.value])
      this.processNext()
    })

    xhr.addEventListener('abort', () => {
      task.xhr = undefined
    })

    const url = `${this.env.apiUrl}/api/files/upload`
    xhr.open('POST', url)
    const token = localStorage.getItem('token')
    if (token) xhr.setRequestHeader('Authorization', `Bearer ${token.trim()}`)
    xhr.send(formData)
  }

  private parseUploadedFile(xhr: XMLHttpRequest): UploadedFileData | undefined {
    try {
      const body = JSON.parse(xhr.responseText)
      const d = body?.data
      if (!d?.id && !d?.Id) return undefined
      return {
        id: d.id ?? d.Id,
        name: d.name ?? d.Name,
        sizeInBytes: d.sizeInBytes ?? d.SizeInBytes ?? 0,
        contentType: d.contentType ?? d.ContentType,
        extension: d.extension ?? d.Extension,
        folderId: d.folderId ?? d.FolderId,
        createdAt: d.createdAt ?? d.CreatedAt ?? new Date().toISOString(),
        createdByName: d.createdByName ?? d.CreatedByName,
      }
    } catch {
      return undefined
    }
  }

  private parseApiFailure(xhr: XMLHttpRequest): string | null {
    try {
      const body = JSON.parse(xhr.responseText)
      if (body?.isSuccess === false && body?.message) {
        return body.message
      }
    } catch { /* not JSON */ }
    return null
  }

  private parseXhrError(xhr: XMLHttpRequest): string {
    try {
      const body = JSON.parse(xhr.responseText)
      if (body?.message) return body.message
      if (body?.errors) {
        const msgs = Object.values(body.errors).flat() as string[]
        if (msgs.length) {
          return msgs
            .map((m) => this.humanizeErrorMessage(m))
            .join('; ')
        }
      }
      if (body?.title) return body.title
    } catch {
      // not JSON
    }
    if (xhr.status === 413) return 'Tệp vượt quá giới hạn cho phép'
    return `Lỗi máy chủ (${xhr.status})`
  }

  private humanizeErrorMessage(msg: string): string {
    const match = msg.match(
      /max request body size is (\d+) bytes/i
    )
    if (match) {
      const bytes = parseInt(match[1], 10)
      return `Tệp vượt quá giới hạn ${this.formatBytes(bytes)}`
    }
    return msg
  }

  private formatBytes(bytes: number): string {
    if (bytes >= 1024 * 1024 * 1024)
      return `${(bytes / (1024 * 1024 * 1024)).toFixed(1)} GB`
    if (bytes >= 1024 * 1024)
      return `${(bytes / (1024 * 1024)).toFixed(0)} MB`
    return `${(bytes / 1024).toFixed(0)} KB`
  }

  private updateTasks(tasks: UploadTask[]): void {
    this.tasksSubject.next(tasks)
  }
}
