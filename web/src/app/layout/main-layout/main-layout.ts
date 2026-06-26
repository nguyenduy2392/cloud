import { Component, HostListener, inject, OnInit, Renderer2 } from '@angular/core'
import { Sidebar } from '../sidebar/sidebar'
import { RouterModule } from '@angular/router'
import { Topbar } from '../topbar/topbar'
import { RightSidebar } from '../right-sidebar/right-sidebar'
import { NgbOffcanvas, NgbOffcanvasModule } from '@ng-bootstrap/ng-bootstrap'
import { LayoutService } from '@/app/services/layout.service'
import { take } from 'rxjs/operators'

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    Sidebar,
    RouterModule,
    Topbar,
    NgbOffcanvasModule,
  ],
  templateUrl: './main-layout.html',
  styles: ``,
})
export class MainLayout implements OnInit {
  year = new Date().getFullYear()
  private offcanvasService = inject(NgbOffcanvas)
  private layoutService = inject(LayoutService)
  private renderer = inject(Renderer2)

  @HostListener('window:resize')
  ngOnInit(): void {
    if (document.documentElement.clientWidth <= 1140) {
      this.onResize()
    }
  }

  onResize() {
    if (
      document.documentElement.clientWidth <= 1140 &&
      document.documentElement.clientWidth >= 768
    ) {
      this.layoutService.changeSidebarSize('condensed')
    } else if (document.documentElement.clientWidth <= 768) {
      this.layoutService.changeSidebarSize('full')
    } else {
      this.layoutService.changeSidebarSize('default')
      document.documentElement.classList.remove('sidebar-enable')
      const backdrop = document.querySelector('.offcanvas-backdrop')
      if (backdrop) this.renderer.removeChild(document.body, backdrop)
    }

    this.layoutService.getSidebarSize$().subscribe((size: string) => {
      this.renderer.setAttribute(
        document.documentElement,
        'data-sidenav-size',
        size
      )
    })
  }
  onSettingsButtonClicked() {
    this.offcanvasService.open(RightSidebar, {
      position: 'end',
      backdrop: true,
    })
  }

  onToggleMobileMenu() {
    this.layoutService.getSidebarSize$().pipe(take(1)).subscribe((size: string) => {
      document.documentElement.setAttribute('data-sidenav-size', size)
    })

    const size = document.documentElement.getAttribute('data-sidenav-size')

    document.documentElement.classList.toggle('sidebar-enable')
    if (size != 'full') {
      if (document.documentElement.classList.contains('sidebar-enable')) {
        this.layoutService.changeSidebarSize('condensed')
      } else {
        this.layoutService.changeSidebarSize('default')
      }
    } else {
      this.showBackdrop()
    }
  }
  showBackdrop() {
    const backdrop = this.renderer.createElement('div')
    this.renderer.addClass(backdrop, 'offcanvas-backdrop')
    this.renderer.addClass(backdrop, 'fade')
    this.renderer.addClass(backdrop, 'show')
    this.renderer.appendChild(document.body, backdrop)
    this.renderer.setStyle(document.body, 'overflow', 'hidden')

    if (window.innerWidth > 1040) {
      this.renderer.setStyle(document.body, 'paddingRight', '15px')
    }

    this.renderer.listen(backdrop, 'click', () => {
      document.documentElement.classList.remove('sidebar-enable')
      this.renderer.removeChild(document.body, backdrop)
      this.renderer.setStyle(document.body, 'overflow', null)
      this.renderer.setStyle(document.body, 'paddingRight', null)
    })
  }
}
