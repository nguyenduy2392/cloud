import { ComponentFixture, TestBed } from '@angular/core/testing'

import { HorizontalLayout } from './horizontal-layout'

describe('HorizontalLayout', () => {
  let component: HorizontalLayout
  let fixture: ComponentFixture<HorizontalLayout>

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HorizontalLayout],
    }).compileComponents()

    fixture = TestBed.createComponent(HorizontalLayout)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
