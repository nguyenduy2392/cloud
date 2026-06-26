import { Injectable } from '@angular/core'

@Injectable({
  providedIn: 'root',
})
export class EnvService {
  get serverUtcOffset(): number {
    return 7
  }

  private get tenant(): any {
    return (window as any).__env?.tenantMap?.[window.location.hostname] ?? {}
  }

  get apiUrl(): string {
    return this.tenant.baseApiUrl ?? (
      window.location.hostname === 'localhost' ? 'http://localhost:5124' : ''
    )
  }

  get ssoUrl(): string {
    return this.tenant.ssoUrl ?? 'http://localhost:4201'
  }

  get ssoApiUrl(): string {
    return this.tenant.ssoApiUrl ?? 'http://localhost:5265'
  }

  get ssoClientId(): string {
    return this.tenant.ssoClientId ?? 'cloud'
  }

  get ssoRedirectUri(): string {
    return this.tenant.ssoRedirectUri ?? 'http://localhost:4202/sso-callback'
  }
}
