import {
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  EventEmitter,
  inject,
  Output,
  type OnInit,
} from '@angular/core'
import { Router, RouterLink } from '@angular/router'
import { NgbDropdownModule, NgbOffcanvasModule } from '@ng-bootstrap/ng-bootstrap'
import { appData, languages } from './data'
import { SimplebarAngularModule } from 'simplebar-angular'
import { splitArray } from '@core/helper/utils'
import { LogoBox } from '@components/logo-box'
import { UserAvatarComponent } from '@/app/components/user-avatar/user-avatar.component'
import { currency } from '@common/constants'
import { LayoutService, type LayoutState } from '@/app/services/layout.service'
import { AuthService, type AuthUser } from '@/app/services/auth.service'
import { SearchService } from '@/app/services/search.service'
import { EnvService } from '@/app/services/env.service'

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [
    NgbOffcanvasModule,
    NgbDropdownModule,
    SimplebarAngularModule,
    RouterLink,
    LogoBox,
    UserAvatarComponent,
  ],
  templateUrl: './topbar.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  styles: ``,
})
export class Topbar implements OnInit {
  currency = currency

  languageList = languages
  appsChunks = splitArray(appData, 3)

  @Output() settingsButtonClicked = new EventEmitter()
  @Output() mobileMenuButtonClicked = new EventEmitter()

  searchService = inject(SearchService)

  router = inject(Router)
  private layoutService = inject(LayoutService)
  private authService = inject(AuthService)
  private envService = inject(EnvService)

  color!: string

  /** User từ `localStorage` (sau đăng nhập) — dùng cho avatar / tên trên topbar */
  sessionUser: AuthUser | null = null

  get avatarSrc(): string | null {
    const a = this.sessionUser?.avatar?.trim()
    if (!a) return null
    if (a.startsWith('http')) return a
    return `${this.envService.apiUrl}/Files/${a}`
  }

  get displayName(): string {
    const n = this.sessionUser?.name?.trim()
    if (n) return n
    const u = this.sessionUser?.userName?.trim()
    if (u) return u
    return 'User'
  }

  /** Dòng phụ: email hoặc mô tả ngắn theo quyền */
  get displaySubtitle(): string {
    const email = this.sessionUser?.email?.trim()
    if (email) return email
    if (this.sessionUser?.isRootAdmin) return 'Administrator'
    return ''
  }

  onSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value.trim()
    this.searchService.set(value)
  }

  ngOnInit(): void {
    this.sessionUser = this.authService.getStoredUser()
    this.layoutService.layoutState$.subscribe((data: LayoutState) => {
      this.color = data.LAYOUT_THEME
    })
  }

  settingMenu() {
    this.settingsButtonClicked.emit()
  }

  toggleMobileMenu() {
    this.mobileMenuButtonClicked.emit()
  }

  changeTheme() {
    const color = document.documentElement.getAttribute('data-bs-theme')
    if (color == 'light') {
      this.layoutService.changeTheme('dark')
    } else {
      this.layoutService.changeTheme('light')
    }
    this.layoutService.getLayoutColor$().subscribe((c) => {
      document.documentElement.setAttribute('data-bs-theme', c)
    })
  }

  logout() {
    this.authService.logout()
    const returnUrl = encodeURIComponent(window.location.origin + '/auth/login')
    window.location.href = `${this.envService.ssoApiUrl}/auth/logout?returnUrl=${returnUrl}`
  }
}
