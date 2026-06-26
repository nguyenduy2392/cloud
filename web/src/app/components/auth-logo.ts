import { CommonModule } from '@angular/common'
import { Component } from '@angular/core'
import { RouterLink } from '@angular/router'

@Component({
  selector: 'app-auth-logo',
  standalone: true,
  imports: [RouterLink, CommonModule],
  template: ` <a routerLink="/" class="auth-brand mb-3">
    <img
      src="../../assets/images/logo-dark.png"
      alt="dark logo"
      height="60"
      class="logo-dark"
    />
    <img
      src="../../assets/images/logo-light.png"
      alt="logo light"
      height="60"
      class="logo-light"
    />
  </a>`,
})
export class AuthLogo {}
