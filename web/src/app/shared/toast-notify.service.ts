import { Injectable } from '@angular/core'

export type ToastType = 'success' | 'warning' | 'danger'

export interface ToastMessage {
  type: ToastType
  title: string
  content: string
}

@Injectable({ providedIn: 'root' })
export class ToastNotifyService {
  private toasts: ToastMessage[] = []

  getMessages(): ToastMessage[] {
    return this.toasts
  }

  success(content: string, title = 'Thành công'): void {
    this.add('success', title, content)
  }

  warning(content: string, title = 'Cảnh báo'): void {
    this.add('warning', title, content)
  }

  danger(content: string, title = 'Lỗi'): void {
    this.add('danger', title, content)
  }

  remove(index: number): void {
    this.toasts.splice(index, 1)
  }

  private add(type: ToastType, title: string, content: string): void {
    this.toasts.push({ type, title, content })
    setTimeout(() => {
      const idx = this.toasts.findIndex((t) => t.title === title && t.content === content)
      if (idx >= 0) this.remove(idx)
    }, 4000)
  }
}
