import { NgComponentOutlet } from '@angular/common'
import { Component, inject, Type } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { DeviceUiService } from '@/app/services/device-ui.service'

/** Khóa trên `Route.data` — dùng cùng {@link DevicePairOutlet}. */
export const DEVICE_PAIR_DATA = {
  mobile: 'mobileCmp',
  desktop: 'desktopCmp',
} as const

export type DevicePairRouteData = {
  [DEVICE_PAIR_DATA.mobile]: Type<unknown>
  [DEVICE_PAIR_DATA.desktop]: Type<unknown>
}

/**
 * Một shell dùng chung: cùng URL, mobile/desktop khác component theo `route.data`.
 *
 * Ví dụ trong `views.route.ts`:
 * `component: DevicePairOutlet, data: { mobileCmp: MFoo, desktopCmp: Foo }`
 */
@Component({
  selector: 'app-device-pair-outlet',
  standalone: true,
  imports: [NgComponentOutlet],
  template: `
    @if (deviceUi.useMobileShell()) {
      <ng-container *ngComponentOutlet="mobileCmp" />
    } @else {
      <ng-container *ngComponentOutlet="desktopCmp" />
    }
  `,
})
export class DevicePairOutlet {
  protected readonly deviceUi = inject(DeviceUiService)
  private readonly route = inject(ActivatedRoute)
  readonly mobileCmp = this.route.snapshot.data[
    DEVICE_PAIR_DATA.mobile
  ] as Type<unknown>
  readonly desktopCmp = this.route.snapshot.data[
    DEVICE_PAIR_DATA.desktop
  ] as Type<unknown>
}
