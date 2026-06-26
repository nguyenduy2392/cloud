import {
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  inject,
  type OnInit,
} from '@angular/core'
import { NgbActiveOffcanvas } from '@ng-bootstrap/ng-bootstrap'
import { SimplebarAngularModule } from 'simplebar-angular'
import { LayoutService } from '@/app/services/layout.service'

@Component({
  selector: 'app-right-sidebar',
  standalone: true,
  imports: [SimplebarAngularModule],
  templateUrl: './right-sidebar.html',
  styles: `
    :host {
      display: contents;
    }
  `,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class RightSidebar implements OnInit {
  offcanvas = inject(NgbActiveOffcanvas)
  private layoutService = inject(LayoutService)

  layout: string = ''
  color!: string
  topbar!: string
  menuColor!: string
  menuSize!: string
  mode!: string

  ngOnInit(): void {
    this.layoutService.layoutState$.subscribe((data) => {
      this.layout = data.LAYOUT
      this.color = data.LAYOUT_THEME
      this.topbar = data.TOPBAR_COLOR
      this.menuColor = data.MENU_COLOR
      this.menuSize = data.MENU_SIZE
      this.mode = data.LAYOUT_MODE
    })
  }

  // Change Layout
  changeLayout(layout: string) {
    this.layout = layout
    this.layoutService.changeLayout(layout)
  }

  changeLayoutColor(color: string) {
    this.layoutService.changeTheme(color)
  }

  changeLayoutMode(mode: string) {
    this.layoutService.changeMode(mode)
  }

  // Change Topbar Color
  changeTopbar(topbar: string) {
    this.layoutService.changeTopbar(topbar)
  }

  // Change Menu Color
  changeMenu(menu: string) {
    this.layoutService.changeMenu(menu)
  }

  // Change Sidebar Size
  changeSize(size: string) {
    this.layoutService.changeSidebarSize(size)
  }

  // Reset Option
  reset() {
    this.layoutService.reset()
  }
}
