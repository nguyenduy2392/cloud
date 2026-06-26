import { isPlatformBrowser } from '@angular/common'
import { Injectable, computed, inject, PLATFORM_ID } from '@angular/core'
import { DeviceDetectorService, DeviceType } from 'ngx-device-detector'

/**
 * Lớp bọc phát hiện thiết bị: dùng chung cho Layout (MobileLayout) và các container mobile/desktop.
 */
@Injectable({ providedIn: 'root' })
export class DeviceUiService {
  private readonly detector = inject(DeviceDetectorService)
  private readonly platformId = inject(PLATFORM_ID)

  /**
   * true khi nên dùng khung MobileLayout (điện thoại + tablet theo ngx-device-detector).
   * SSR: luôn false để hydrate an toàn với desktop shell nếu cần.
   */
  readonly useMobileShell = computed(() => {
    if (!isPlatformBrowser(this.platformId)) {
      return false
    }
    const t = this.detector.deviceType()
    return t === DeviceType.Mobile || t === DeviceType.Tablet
  })
}
