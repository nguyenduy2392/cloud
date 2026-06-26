import {
  Component,
  inject,
  Renderer2,
  type OnDestroy,
  type OnInit,
} from '@angular/core'  
import { RouterModule } from '@angular/router'
import { MobileTopbar } from './mobile-topbar/mobile-topbar'
import { MobileBottomNav } from './mobile-bottom-nav/mobile-bottom-nav'
import { MOBILE_APP_NAV_ITEMS } from './mobile-menu-meta'

@Component({
  selector: 'mobile-layout',
  standalone: true,
  imports: [MobileTopbar, RouterModule, MobileBottomNav],
  templateUrl: './mobile-layout.html',
  styles: `
    .mobile-shell--root {
      height: 100vh;
      height: 100dvh;
      max-height: 100vh;
      max-height: 100dvh;
      overflow: hidden;
      background: #fff !important;
    }

    .mobile-shell__top {
      position: relative;
      z-index: 1030;
    }

    .mobile-shell__scroll {
      overflow-y: auto;
      overflow-x: hidden;
      -webkit-overflow-scrolling: touch;
      overscroll-behavior-y: contain;
    }

    .mobile-shell--bottom-nav {
      /* thanh + FAB nổi + safe area */
      padding-bottom: 4rem;
    }
  `,
})
export class MobileLayout implements OnInit, OnDestroy {
  private render = inject(Renderer2)

  readonly hasBottomNav = MOBILE_APP_NAV_ITEMS.length > 0

  ngOnInit(): void {
    this.render.setAttribute(document.documentElement, 'data-layout', 'mobile')
  }

  ngOnDestroy(): void {
    this.render.removeAttribute(document.documentElement, 'data-layout')
  }
}
