import { Injectable } from '@angular/core'
import { BehaviorSubject } from 'rxjs'

@Injectable({
  providedIn: 'root',
})
export class ToggleService {
  private toggleState = new BehaviorSubject<boolean>(false)
  toggleState$ = this.toggleState.asObservable()

  toggle() {
    this.toggleState.next(!this.toggleState.value)
  }

  setToggleState(state: boolean) {
    this.toggleState.next(state)
  }

  open() {
    this.toggleState.next(true)
  }

  close() {
    this.toggleState.next(false)
  }
}
