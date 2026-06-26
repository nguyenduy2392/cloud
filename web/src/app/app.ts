import { Component, inject, OnInit, ViewChild } from '@angular/core'
import {
  NavigationCancel,
  NavigationEnd,
  NavigationError,
  NavigationStart,
  Router,
  RouterOutlet,
  type Event,
} from '@angular/router'
import { TitleService } from '@core/services/title.service'
import { NgProgressbar, NgProgressRef } from 'ngx-progressbar'
import { UiModalHostComponent } from '@/app/components/ui-modal-host/ui-modal-host.component'
import { ToastNotifyComponent } from '@/app/shared/toast-notify.component'
import { UploadWorkerComponent } from '@/app/components/upload-worker/upload-worker.component'

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NgProgressbar, UiModalHostComponent, ToastNotifyComponent, UploadWorkerComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit {
  private titleService = inject(TitleService)
  progressRef!: NgProgressRef
  @ViewChild(NgProgressRef) progressBar!: NgProgressRef

  private router = inject(Router)

  constructor() {
    this.router.events.subscribe((event: Event) => {
      this.checkRouteChange(event)
    })
  }

  ngOnInit(): void {
    this.titleService.init()
  }

  checkRouteChange(routerEvent: Event) {
    if (routerEvent instanceof NavigationStart) {
      this.progressBar.start()
    }
    if (
      routerEvent instanceof NavigationEnd ||
      routerEvent instanceof NavigationCancel ||
      routerEvent instanceof NavigationError
    ) {
      setTimeout(() => {
        this.progressBar.complete()
      }, 200)
    }
  }
}
