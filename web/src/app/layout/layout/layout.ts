import { Component, inject, type OnInit } from '@angular/core'
import { MainLayout } from '../main-layout/main-layout'
import { HorizontalLayout } from '../horizontal-layout/horizontal-layout'
import { MobileLayout } from '../mobile-layout/mobile-layout'
import { LayoutService, type LayoutState } from '@/app/services/layout.service'
import { DeviceUiService } from '@/app/services/device-ui.service'

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [MainLayout, HorizontalLayout, MobileLayout],
  templateUrl: './layout.html',
  styles: ``,
})
export class Layout implements OnInit {
  layoutType: LayoutState['LAYOUT'] = ''

  protected readonly deviceUi = inject(DeviceUiService)
  private layoutService = inject(LayoutService)

  ngOnInit(): void {
    this.layoutService.layoutState$.subscribe((data: LayoutState) => {
      this.layoutType = data.LAYOUT
    })
  }

  /**
   * Check if the horizontal layout is requested
   */
  isHorizontalLayoutRequested() {
    return this.layoutType === 'horizontal'
  }
  /**
   * Check if the vertical layout is requested
   */
  isVerticalLayoutRequested() {
    return this.layoutType === 'vertical'
  }
}
