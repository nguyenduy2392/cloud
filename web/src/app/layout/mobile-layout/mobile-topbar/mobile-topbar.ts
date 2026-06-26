import {
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  DestroyRef,
  inject,
  type OnInit,
} from '@angular/core'
import { takeUntilDestroyed } from '@angular/core/rxjs-interop'
import {
  NavigationEnd,
  NavigationStart,
  Router,
} from '@angular/router'
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap'
import { AuthService, type AuthUser } from '@/app/services/auth.service'
import { EnvService } from '@/app/services/env.service'
@Component({
  selector: 'app-mobile-topbar',
  standalone: true,
  imports: [
    NgbDropdownModule,
  ],
  templateUrl: './mobile-topbar.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  styleUrl: './mobile-topbar.scss',
})
export class MobileTopbar implements OnInit {
  router = inject(Router)
  private readonly destroyRef = inject(DestroyRef)
  private authService = inject(AuthService)
  private envService = inject(EnvService)
  /** Lấy từ route lá cùng `data.title` / `data.subtitle` */
  pageTitle = ''
  pageSubtitle = ''

  sessionUser: AuthUser | null = null

  /** URL đã qua (sau redirect); dùng để xây stack khi điều hướng tới */
  private lastTrackedUrl = ''
  /** Các URL có thể quay lại (từ dưới lên = màn ngay trước) */
  private backStack: string[] = []
  /** Bỏ qua push stack khi vừa dùng nút back trong app */
  private skippingHistoryPush = false
  private lastNavTrigger: NavigationStart['navigationTrigger'] | null = null

  get canGoBack(): boolean {
    return this.backStack.length > 0
  }

  get avatarSrc(): string {
    const a = this.sessionUser?.avatar?.trim()
    if (!a) return 'assets/images/users/avatar-1.jpg'
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

  get displaySubtitle(): string {
    const email = this.sessionUser?.email?.trim()
    if (email) return email
    if (this.sessionUser?.isRootAdmin) return 'Administrator'
    return ''
  }

  ngOnInit(): void {
    this.sessionUser = this.authService.getStoredUser()
    this.router.events
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((e) => {
        if (e instanceof NavigationStart) {
          this.lastNavTrigger = e.navigationTrigger ?? null
        }
        if (e instanceof NavigationEnd) {
          this.onNavigationEnd(e)
        }
      })
    this.syncPageTitle()
  }

  goBack(): void {
    const prev = this.backStack.pop()
    if (!prev) return
    this.skippingHistoryPush = true
    void this.router.navigateByUrl(prev)
  }

  private onNavigationEnd(e: NavigationEnd): void {
    const url = e.urlAfterRedirects

    if (this.skippingHistoryPush) {
      this.skippingHistoryPush = false
      this.lastTrackedUrl = url
      this.syncPageTitle()
      return
    }

    if (this.lastNavTrigger === 'popstate') {
      if (this.backStack.length > 0) {
        this.backStack.pop()
      }
      this.lastTrackedUrl = url
      this.syncPageTitle()
      return
    }

    if (this.lastTrackedUrl && this.lastTrackedUrl !== url) {
      this.backStack.push(this.lastTrackedUrl)
    }
    this.lastTrackedUrl = url
    this.syncPageTitle()
  }

  private syncPageTitle(): void {
    let r = this.router.routerState.snapshot.root
    while (r.firstChild) {
      r = r.firstChild
    }
    const data = r.data as Record<string, unknown>
    const t = data['title']
    const s = data['subtitle']
    this.pageTitle = typeof t === 'string' ? t : ''
    this.pageSubtitle = typeof s === 'string' ? s : ''
  }

  logout() {
    this.authService.logout()
    void this.router.navigate(['/auth/login'])
  }
}
