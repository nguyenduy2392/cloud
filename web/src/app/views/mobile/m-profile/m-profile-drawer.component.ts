import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { Router } from '@angular/router'
import { NgbActiveOffcanvas } from '@ng-bootstrap/ng-bootstrap'
import { AuthService, type AuthUser } from '@/app/services/auth.service'
import { EnvService } from '@/app/services/env.service'
import { fromServerTime } from '@/app/shared/server-time'

/**
 * Drawer “Khác” — thông tin user đăng nhập + đăng xuất.
 * Mở từ {@link MobileBottomNav} (offcanvas `position: end`).
 */
@Component({
  selector: 'app-m-profile-drawer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './m-profile-drawer.component.html',
  styleUrl: './m-profile-drawer.component.scss',
})
export class MProfileDrawerComponent implements OnInit {
  private auth = inject(AuthService)
  private env = inject(EnvService)
  private router = inject(Router)
  private activeOffcanvas = inject(NgbActiveOffcanvas)

  user: AuthUser | null = null
  readonly genderLabels: Record<number, string> = {
    0: 'Không xác định',
    1: 'Nam',
    2: 'Nữ',
  }

  ngOnInit(): void {
    // Nguồn chính: localStorage key `user` (xem AuthService.getStoredUser)
    this.user = this.auth.getStoredUser()
    if (!this.user && this.auth.currentUser) {
      this.user = this.auth.currentUser as AuthUser
    }
  }

  get avatarSrc(): string {
    const a = this.user?.avatar?.trim()
    if (!a) return 'assets/images/users/avatar-1.jpg'
    if (a.startsWith('http')) return a
    return `${this.env.apiUrl}/Files/${a}`
  }

  get displayName(): string {
    const n = this.user?.name?.trim()
    if (n) return n
    const u = this.user?.userName?.trim()
    if (u) return u
    return 'Người dùng'
  }

  formatBirthday(iso: string | null | undefined): string {
    if (!iso) return '—'
    const d = fromServerTime(iso, this.env.serverUtcOffset)
    if (Number.isNaN(d.getTime())) return '—'
    const p = (n: number) => String(n).padStart(2, '0')
    return `${p(d.getDate())}/${p(d.getMonth() + 1)}/${d.getFullYear()}`
  }

  genderLabel(v: number | null | undefined): string {
    if (v == null) return '—'
    return this.genderLabels[v] ?? String(v)
  }

  close(): void {
    this.activeOffcanvas.dismiss()
  }

  logout(): void {
    this.auth.logout()
    this.activeOffcanvas.dismiss()
    void this.router.navigate(['/auth/login'])
  }
}
