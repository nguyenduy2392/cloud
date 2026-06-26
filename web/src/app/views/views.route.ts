import type { Route } from '@angular/router'
import { HomeComponent } from './pages/home/home.component'
import { MyCloudComponent } from './pages/my-cloud/my-cloud.component'

export const VIEWS_ROUTES: Route[] = [
  {
    path: '',
    component: HomeComponent,
    data: { title: 'Home' },
    pathMatch: 'full',
  },
  {
    path: 'my-cloud',
    component: MyCloudComponent,
    data: { title: 'Cloud của tôi' },
  },
  {
    path: 'folder/:id',
    component: MyCloudComponent,
    data: { title: 'Cloud của tôi' },
  },
  {
    path: 'shared-with-me',
    component: MyCloudComponent,
    data: { title: 'Chia sẻ với tôi', mode: 'shared' },
  },
  {
    path: 'recent',
    component: MyCloudComponent,
    data: { title: 'Gần đây' },
  },
  { path: '**', redirectTo: '' },
]
