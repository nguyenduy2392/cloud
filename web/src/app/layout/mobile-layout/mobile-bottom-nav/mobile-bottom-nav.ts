import { NgTemplateOutlet } from '@angular/common'
import { Component, inject } from '@angular/core'
import { RouterLink, RouterLinkActive } from '@angular/router'
import { NgbOffcanvas, NgbOffcanvasModule } from '@ng-bootstrap/ng-bootstrap'
import { MProfileDrawerComponent } from '@/app/views/mobile/m-profile/m-profile-drawer.component'
import {
  MOBILE_APP_NAV_ITEMS,
  type MobileNavItem,
} from '../mobile-menu-meta'

function splitNavItems(items: MobileNavItem[]): {
  left: MobileNavItem[]
  center: MobileNavItem | null
  right: MobileNavItem[]
} {
  const idx = items.findIndex((i) => i.center)
  if (idx === -1) {
    const mid = Math.floor(items.length / 2)
    return {
      left: items.slice(0, mid),
      center: null,
      right: items.slice(mid),
    }
  }
  return {
    left: items.slice(0, idx),
    center: items[idx] ?? null,
    right: items.slice(idx + 1),
  }
}

@Component({
  selector: 'mobile-bottom-nav',
  standalone: true,
  imports: [NgTemplateOutlet, RouterLink, RouterLinkActive, NgbOffcanvasModule],
  templateUrl: './mobile-bottom-nav.html',
  styleUrl: './mobile-bottom-nav.scss',
})
export class MobileBottomNav {
  private ngbOffcanvas = inject(NgbOffcanvas)

  items: MobileNavItem[] = MOBILE_APP_NAV_ITEMS

  get leftItems(): MobileNavItem[] {
    return splitNavItems(this.items).left
  }

  get centerItem(): MobileNavItem | null {
    return splitNavItems(this.items).center
  }

  get rightItems(): MobileNavItem[] {
    return splitNavItems(this.items).right
  }

  /** Drawer "Khac": ho so + dang xuat (truot tu phai). */
  openProfileDrawer(): void {
    this.ngbOffcanvas.dismiss()
    this.ngbOffcanvas.open(MProfileDrawerComponent, {
      position: 'end',
      backdrop: 'static',
      scroll: true,
      panelClass: 'm-profile-drawer-offcanvas',
    })
  }

}
