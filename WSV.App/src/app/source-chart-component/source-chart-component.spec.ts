import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SourceChartComponent } from './source-chart-component';

describe('SourceChartComponent', () => {
  let component: SourceChartComponent;
  let fixture: ComponentFixture<SourceChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SourceChartComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SourceChartComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
