import { Injectable } from '@angular/core'
import { BehaviorSubject, combineLatest, Observable, Subject } from 'rxjs'
import { take } from 'rxjs/operators'
import type { UiModal } from '@/app/models/ui-modal.interface'

@Injectable({ providedIn: 'root' })
export class UiModalService {
  private visibleSubject = new BehaviorSubject<boolean>(false)
  readonly visible$ = this.visibleSubject.asObservable()

  private modalSubject = new BehaviorSubject<UiModal | null>(null)
  readonly modal$ = this.modalSubject.asObservable()

  private afterOpenSubject = new Subject<void>()
  readonly afterOpen$ = this.afterOpenSubject.asObservable()

  private afterCloseSubject = new Subject<unknown>()

  /** Gộp trạng thái cho host (signal/async pipe). */
  readonly shell$ = combineLatest({
    visible: this.visibleSubject,
    modal: this.modalSubject,
  })

  openModal(modal: UiModal): void {
    this.modalSubject.next(modal)
    this.visibleSubject.next(true)
  }

  closeModal(result?: unknown): void {
    this.visibleSubject.next(false)
    this.modalSubject.next(null)
    this.afterCloseSubject.next(result)
  }

  /**
   * Mở modal và trả về stream kết quả đóng (lần đóng **tiếp theo**).
   * Tránh chồng nhiều modal cùng lúc.
   */
  create(modalOptions: UiModal): { afterClose: Observable<unknown> } {
    const afterClose = this.afterCloseSubject.pipe(take(1))
    this.openModal(modalOptions)
    this.afterOpenSubject.next()
    return { afterClose }
  }
}
