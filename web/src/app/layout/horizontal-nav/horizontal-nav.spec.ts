import { ComponentFixture, TestBed } from '@angular/core/testing'

import { HorizontalNav } from './horizontal-nav'

describe('HorizontalNav', () => {
  let component: HorizontalNav
  let fixture: ComponentFixture<HorizontalNav>

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HorizontalNav],
    }).compileComponents()

    fixture = TestBed.createComponent(HorizontalNav)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
