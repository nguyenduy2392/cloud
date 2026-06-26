import { Injectable } from '@angular/core'
import { BehaviorSubject, Observable } from 'rxjs'
import { map } from 'rxjs/operators'
import {
  LAYOUT_COLOR_TYPES,
  LAYOUT_MODE_TYPES,
  LAYOUT_TYPES,
  MENU_COLOR_TYPES,
  SIDEBAR_SIZE_TYPES,
  TOPBAR_COLOR_TYPES,
} from '@/app/shared/layout-constants'

const STORAGE_KEY = 'cloud-layout-state'

export interface LayoutState {
  LAYOUT: string
  LAYOUT_THEME: string
  LAYOUT_MODE: string
  TOPBAR_COLOR: string
  MENU_COLOR: string
  MENU_SIZE: string
}

const initialState: LayoutState = {
  LAYOUT: LAYOUT_TYPES.VERTICAL,
  LAYOUT_THEME: LAYOUT_COLOR_TYPES.LIGHTMODE,
  LAYOUT_MODE: LAYOUT_MODE_TYPES.FLUIDMODE,
  TOPBAR_COLOR: TOPBAR_COLOR_TYPES.LIGHT,
  MENU_COLOR: MENU_COLOR_TYPES.LIGHT,
  MENU_SIZE: SIDEBAR_SIZE_TYPES.DEFAULT,
}

function loadState(): LayoutState {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return { ...initialState }
    const parsed = JSON.parse(raw) as Partial<LayoutState>
    return { ...initialState, ...parsed }
  } catch {
    return { ...initialState }
  }
}

function persistState(state: LayoutState): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state))
  } catch {
    // ignore
  }
}

/** Đồng bộ state đã lưu (hoặc mặc định) lên `<html>` — dùng khi khởi động và sau mỗi lần đổi tùy chỉnh. */
function applyLayoutStateToDocument(state: LayoutState): void {
  if (typeof document === 'undefined') return
  const el = document.documentElement
  el.setAttribute('data-bs-theme', state.LAYOUT_THEME)
  el.setAttribute('data-layout-mode', state.LAYOUT_MODE)
  el.setAttribute('data-menu-color', state.MENU_COLOR)
  el.setAttribute('data-topbar-color', state.TOPBAR_COLOR)
  el.setAttribute('data-sidenav-size', state.MENU_SIZE)
  if (state.LAYOUT === LAYOUT_TYPES.HORIZONTAL) {
    el.setAttribute('data-layout', 'topnav')
  } else {
    el.removeAttribute('data-layout')
  }
}

@Injectable({ providedIn: 'root' })
export class LayoutService {
  private readonly _state = new BehaviorSubject<LayoutState>(loadState())

  /** Trạng thái layout đầy đủ (đồng bộ theme / menu / sidebar). */
  readonly layoutState$: Observable<LayoutState> = this._state.asObservable()

  get state(): LayoutState {
    return this._state.value
  }

  changeLayout(layout: string): void {
    this.patch({ LAYOUT: layout })
  }

  changeTheme(color: string): void {
    const menuColor = color === 'dark' ? MENU_COLOR_TYPES.DARK : MENU_COLOR_TYPES.LIGHT
    const topbarColor = color === 'dark' ? TOPBAR_COLOR_TYPES.DARK : TOPBAR_COLOR_TYPES.LIGHT
    this.patch({ LAYOUT_THEME: color, MENU_COLOR: menuColor, TOPBAR_COLOR: topbarColor })
  }

  changeMode(mode: string): void {
    this.patch({ LAYOUT_MODE: mode })
  }

  changeTopbar(topbar: string): void {
    this.patch({ TOPBAR_COLOR: topbar })
  }

  changeMenu(menu: string): void {
    this.patch({ MENU_COLOR: menu })
  }

  changeSidebarSize(size: string): void {
    this.patch({ MENU_SIZE: size })
  }

  reset(): void {
    this._state.next({ ...initialState })
    persistState(this._state.value)
    applyLayoutStateToDocument(this._state.value)
  }

  /** Áp dụng state hiện tại lên DOM (sau khi load từ localStorage hoặc khi cần đồng bộ thủ công). */
  applyLayoutToDocument(): void {
    applyLayoutStateToDocument(this._state.value)
  }

  /** Tương đương selector getLayoutColor */
  getLayoutColor$(): Observable<string> {
    return this._state.pipe(map((s) => s.LAYOUT_THEME))
  }

  getLayoutMode$(): Observable<string> {
    return this._state.pipe(map((s) => s.LAYOUT_MODE))
  }

  getTopbarColor$(): Observable<string> {
    return this._state.pipe(map((s) => s.TOPBAR_COLOR))
  }

  getMenuColor$(): Observable<string> {
    return this._state.pipe(map((s) => s.MENU_COLOR))
  }

  getSidebarSize$(): Observable<string> {
    return this._state.pipe(map((s) => s.MENU_SIZE))
  }

  private patch(partial: Partial<LayoutState>): void {
    const next = { ...this._state.value, ...partial }
    this._state.next(next)
    persistState(next)
    applyLayoutStateToDocument(next)
  }
}
