import { Component } from '@angular/core'
import { RouterLink } from '@angular/router'
import { credits, currentYear } from '@common/constants'

@Component({
  selector: 'app-logout',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './app-logout.html',
  styles: ``,
})
export class AppLogout {
  currentYear = currentYear
  credits = credits
}
