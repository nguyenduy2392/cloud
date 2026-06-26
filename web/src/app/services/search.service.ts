import { Injectable, signal } from '@angular/core'

@Injectable({ providedIn: 'root' })
export class SearchService {
  /** Từ khóa tìm kiếm toàn app */
  term = signal('')

  set(value: string) {
    this.term.set(value)
  }

  clear() {
    this.term.set('')
  }
}
