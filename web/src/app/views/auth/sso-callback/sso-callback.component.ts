import { Component, OnInit } from '@angular/core'
import { ActivatedRoute, Router } from '@angular/router'
import { HttpClient } from '@angular/common/http'
import { CommonModule } from '@angular/common'
import { EnvService } from '@/app/services/env.service'

@Component({
  standalone: true,
  selector: 'app-sso-callback',
  imports: [CommonModule],
  template: `
    <div
      style="display:flex;align-items:center;justify-content:center;height:100vh;flex-direction:column;gap:16px;font-family:sans-serif;"
    >
      <ng-container *ngIf="!errorMsg">
        <div class="spinner-border text-primary"></div>
        <div style="font-size:16px;color:#555;">
          Đang xử lý đăng nhập SSO...
        </div>
      </ng-container>
      <ng-container *ngIf="errorMsg">
        <div
          style="color:red;font-size:15px;text-align:center;max-width:400px;"
        >
          {{ errorMsg }}
        </div>
        <button
          (click)="retry()"
          style="padding:8px 24px;background:#1976d2;color:#fff;border:none;border-radius:4px;cursor:pointer;font-size:14px;"
        >
          Đăng nhập lại
        </button>
      </ng-container>
    </div>
  `,
})
export class SsoCallbackComponent implements OnInit {
  errorMsg = ''

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private env: EnvService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const code = params['code'] ?? ''

      if (!code) {
        this.errorMsg = 'Không tìm thấy authorization code.'
        return
      }

      this.http
        .post<any>(`${this.env.apiUrl}/api/auth/sso-callback`, { code })
        .subscribe({
          next: (res) => {
            if (res?.isSuccess) {
              const token = res.data?.Token || res.data?.token
              if (token) localStorage.setItem('token', token.trim())
              if (res.data?.user)
                localStorage.setItem('user', JSON.stringify(res.data.user))
              if (res.data?.Database)
                localStorage.setItem('database', res.data.Database)
              void this.router.navigate(['/'])
            } else {
              this.errorMsg = res?.message || 'Đăng nhập SSO thất bại.'
            }
          },
          error: (err) => {
            console.error('SSO callback error:', err)
            const msg =
              err?.error?.message ?? err?.error?.title ?? err?.message ?? ''
            this.errorMsg =
              msg || `Lỗi kết nối máy chủ (${err?.status ?? 'unknown'}).`
          },
        })
    })
  }

  retry(): void {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    localStorage.removeItem('database')
    const random = Math.random().toString(36).slice(2)
    const authorizeUrl = `${this.env.ssoApiUrl}/auth/authorize?client_id=${this.env.ssoClientId}&redirect_uri=${encodeURIComponent(this.env.ssoRedirectUri)}&state=${random}`
    window.location.href = `${this.env.ssoUrl}/auth/login?returnUrl=${encodeURIComponent(authorizeUrl)}`
  }
}
