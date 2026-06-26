import {
  Component,
  inject,
  Renderer2,
  type OnDestroy,
  type OnInit,
} from '@angular/core'
import { Topbar } from '../topbar/topbar'
import { NgbOffcanvas } from '@ng-bootstrap/ng-bootstrap'
import { RouterModule } from '@angular/router'
import { Footer } from '../footer/footer'
import { RightSidebar } from '../right-sidebar/right-sidebar'
import { HorizontalNav } from '../horizontal-nav/horizontal-nav'

@Component({
  selector: 'horizontal-layout',
  standalone: true,
  imports: [
    Topbar,
    RouterModule,
    Footer,
    HorizontalNav,
  ],
  templateUrl: './horizontal-layout.html',
  styles: ``,
})
export class HorizontalLayout implements OnInit, OnDestroy {
  private offcanvasService = inject(NgbOffcanvas)
  private render = inject(Renderer2)

  ngOnInit(): void {
    this.render.setAttribute(document.documentElement, 'data-layout', 'topnav')
  }

  ngOnDestroy(): void {
    this.render.removeAttribute(document.documentElement, 'data-layout')
  }

  onSettingsButtonClicked() {
    this.offcanvasService.open(RightSidebar, {
      position: 'end',
      backdrop: true,
    })
  }

  onToggleMobileMenu() {
    document.getElementById('topnav-menu-content')?.classList.toggle('show')
  }
}
