import { Directive, ElementRef, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core'
import flatpickr from 'flatpickr'
import { Options } from 'flatpickr/dist/types/options'

@Directive({
  selector: '[mwlFlatpickr]',
  standalone: true,
})
export class FlatpickrDirective implements OnInit, OnChanges {
  /** Cập nhật khi giá trị mặc định thay đổi (sau khi API trả về). */
  @Input() flatpickrDefaultDate: string | Date | null = null

  @Input() flatpickrOptions: Options = {}

  private instance: flatpickr.Instance | null = null

  constructor(private el: ElementRef) {}

  ngOnInit() {
    this.initFlatpickr()
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['flatpickrDefaultDate'] && this.instance) {
      const newVal = changes['flatpickrDefaultDate'].currentValue
      this.instance.setDate(newVal, true)
    }
    if (changes['flatpickrOptions'] && this.instance) {
      this.instance.set(changes['flatpickrOptions'].currentValue)
    }
  }

  private initFlatpickr() {
    const opts: Options = { ...this.flatpickrOptions }
    if (this.flatpickrDefaultDate) {
      opts.defaultDate = this.flatpickrDefaultDate
    }
    this.instance = flatpickr(this.el.nativeElement, opts)
  }
}
