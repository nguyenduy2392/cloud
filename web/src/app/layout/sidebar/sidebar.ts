import { Component, inject, OnInit } from '@angular/core'
import { NgbCollapse, NgbCollapseModule } from '@ng-bootstrap/ng-bootstrap'
import { NavigationEnd, Router, RouterModule } from '@angular/router'
import { CommonModule } from '@angular/common'
import { SimplebarAngularModule } from 'simplebar-angular'
import { MENU_ITEMS, MenuItemType } from '@common/menu-meta'
import { basePath } from '@common/constants'
import { findAllParent, findMenuItem } from '@core/helper/utils'
import { LogoBox } from '@components/logo-box'
import { UserStorageService, type UserStorage } from '@/app/services/user-storage.service'
import { formatFileSize } from '@/app/shared/file-size.pipe'

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    NgbCollapseModule,
    SimplebarAngularModule,
    LogoBox,
  ],
  templateUrl: './sidebar.html',
  styles: ``,
})
export class Sidebar implements OnInit {
  menuItems: MenuItemType[] = []
  activeMenuItems: string[] = []

  storageInfo: UserStorage | null = null
  storageUsageLabel = ''
  storagePercent = 0
  storageBarClass = 'bg-primary'

  router = inject(Router)
  private userStorageService = inject(UserStorageService)
  trimmedURL = this.router.url?.replaceAll(
    basePath !== '' ? basePath + '/' : '',
    '/'
  )

  constructor() {
    this.router.events.forEach((event) => {
      if (event instanceof NavigationEnd) {
        this.trimmedURL = this.router.url?.replaceAll(
          basePath !== '' ? basePath + '/' : '',
          '/'
        )
        this._activateMenu()
        setTimeout(() => {
          this.scrollToActive()
        }, 200)
      }
    })
  }

  ngOnInit(): void {
    this.initMenu()
    this.userStorageService.storage$.subscribe({
      next: (res) => {
        if (res?.isSuccess && res.data) {
          this.storageInfo = res.data
          const pct = res.data.maxBytes > 0
            ? Math.min(100, (res.data.usedBytes / res.data.maxBytes) * 100)
            : 0
          this.storagePercent = pct
          this.storageUsageLabel = `${formatFileSize(res.data.usedBytes)} / ${formatFileSize(res.data.maxBytes)}`
          this.storageBarClass = pct >= 90 ? 'bg-danger' : pct >= 70 ? 'bg-warning' : 'bg-primary'
        }
      },
    })
  }

  initMenu(): void {
    this.menuItems = [...MENU_ITEMS]
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this._activateMenu()
    })
    setTimeout(() => {
      this.scrollToActive()
    }, 200)
  }

  hasSubmenu(menu: MenuItemType): boolean {
    return menu.children ? true : false
  }

  scrollToActive(): void {
    const activatedItem = document.querySelector('.side-nav-item li a.active')
    if (activatedItem) {
      const simplebarContent = document.querySelector(
        '.sidenav-menu .simplebar-content-wrapper'
      )
      if (simplebarContent) {
        const activatedItemRect = activatedItem.getBoundingClientRect()
        const simplebarContentRect = simplebarContent.getBoundingClientRect()
        const activatedItemOffsetTop =
          activatedItemRect.top + simplebarContent.scrollTop
        const centerOffset =
          activatedItemOffsetTop -
          simplebarContentRect.top -
          simplebarContent.clientHeight / 2 +
          activatedItemRect.height / 2
        this.scrollTo(simplebarContent, centerOffset, 600)
      }
    }
  }

  easeInOutQuad(t: number, b: number, c: number, d: number): number {
    t /= d / 2
    if (t < 1) return (c / 2) * t * t + b
    t--
    return (-c / 2) * (t * (t - 2) - 1) + b
  }

  scrollTo(element: Element, to: number, duration: number): void {
    const start = element.scrollTop
    const change = to - start
    const increment = 20
    let currentTime = 0

    const animateScroll = () => {
      currentTime += increment
      const val = this.easeInOutQuad(currentTime, start, change, duration)
      element.scrollTop = val
      if (currentTime < duration) {
        setTimeout(animateScroll, increment)
      }
    }
    animateScroll()
  }

  _activateMenu(): void {
    const div = document.querySelector('.sidenav-menu')

    let matchingMenuItem = null

    if (div) {
      let items: any = div.getElementsByClassName('nav-link-ref')
      for (let i = 0; i < items.length; ++i) {
        if (
          this.trimmedURL === items[i].pathname ||
          (this.trimmedURL.startsWith('/invoice/') &&
            items[i].pathname === '/invoice/RB6985') ||
          (this.trimmedURL.startsWith('/ecommerce/product/') &&
            items[i].pathname === '/ecommerce/product/1')
        ) {
          matchingMenuItem = items[i]
          break
        }
      }

      if (matchingMenuItem) {
        const mid = matchingMenuItem.getAttribute('aria-controls')
        const activeMt = findMenuItem(this.menuItems, mid)

        if (activeMt) {
          const matchingObjs = [
            activeMt['key'],
            ...findAllParent(this.menuItems, activeMt),
          ]

          this.activeMenuItems = matchingObjs
          this.menuItems.forEach((menu: MenuItemType) => {
            menu.collapsed = !matchingObjs.includes(menu.key!)
          })
        }
      }
    }
  }

  /**
   * toggles open menu
   * @param menuItem clicked menuitem
   * @param collapse collpase instance
   */
  toggleMenuItem(menuItem: MenuItemType, collapse: NgbCollapse): void {
    collapse.toggle()
    let openMenuItems: string[]
    if (!menuItem.collapsed) {
      openMenuItems = [
        menuItem['key'],
        ...findAllParent(this.menuItems, menuItem),
      ]
      this.menuItems.forEach((menu: MenuItemType) => {
        if (!openMenuItems.includes(menu.key!)) {
          menu.collapsed = true
        }
      })
    }
  }

}
