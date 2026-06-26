import {
  APP_INITIALIZER,
  ApplicationConfig,
  importProvidersFrom,
  provideZoneChangeDetection,
  isDevMode,
} from '@angular/core'
import { provideRouter } from '@angular/router'
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'

import { routes } from './app.routes'
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withFetch,
  withInterceptorsFromDi,
} from '@angular/common/http'
import { FakeBackendProvider } from '@core/helper/fake-backend'
import { DecimalPipe } from '@angular/common'
import { BrowserModule } from '@angular/platform-browser'
import { ErrorInterceptor } from '@core/helper/error.interceptor'
import { LayoutService } from '@/app/services/layout.service'
import { provideServiceWorker } from '@angular/service-worker'

function initLayoutFromStorage(layout: LayoutService) {
  return () => {
    layout.applyLayoutToDocument()
  }
}

export const appConfig: ApplicationConfig = {
  providers: [
    {
      provide: APP_INITIALIZER,
      useFactory: initLayoutFromStorage,
      deps: [LayoutService],
      multi: true,
    },
    FakeBackendProvider,
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    DecimalPipe,
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    importProvidersFrom(BrowserAnimationsModule, BrowserModule),
    provideHttpClient(withFetch(), withInterceptorsFromDi()),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),
  ],
}
