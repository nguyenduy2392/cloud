import { CommonModule } from '@angular/common'
import { Component } from '@angular/core'
import { RouterLink } from '@angular/router'

@Component({
  selector: 'third-party-login',
  standalone: true,
  imports: [RouterLink, CommonModule],
  template: ` <div class="d-flex justify-content-center gap-2 mb-3">
    <a href="javascript: void(0);" class="btn btn-soft-danger avatar-lg"
      ><i class="ti ti-brand-google-filled fs-24"></i
    ></a>
    <a href="javascript: void(0);" class="btn btn-soft-success avatar-lg"
      ><i class="ti ti-brand-apple fs-24"></i
    ></a>
    <a href="javascript: void(0);" class="btn btn-soft-primary avatar-lg"
      ><i class="ti ti-brand-facebook fs-24"></i
    ></a>
    <a href="javascript: void(0);" class="btn btn-soft-info avatar-lg"
      ><i class="ti ti-brand-linkedin fs-24"></i
    ></a>
  </div>`,
})
export class ThirdPartyLogin {}
