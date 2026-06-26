import { CommonModule } from '@angular/common'
import { Component } from '@angular/core'
import { RouterLink } from '@angular/router'

@Component({
  selector: 'app-logo-box',
  standalone: true,
  imports: [RouterLink, CommonModule],
  template: ` <a routerLink="/" class="logo">
    <span class="logo-light">
      <span class="logo-lg"
        ><img style="height: 40px !important;" src="../../assets/images/logo-light.png" alt="logo"
      /></span>
      <span class="logo-sm"
        ><img src="../../assets/images/logo-light.png" alt="small logo"
      /></span>
    </span>

    <span class="logo-dark">
      <span class="logo-lg"
        ><img style="height: 40px !important;" src="../../assets/images/logo-dark.png" alt="dark logo"
      /></span>
      <span class="logo-sm"
        ><img src="../../assets/images/logo-dark.png" alt="small logo"
      /></span>
    </span>
  </a>`,
})
export class LogoBox {}
