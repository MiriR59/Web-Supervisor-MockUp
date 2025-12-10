import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SourceChart } from './source-chart';

describe('SourceChart', () => {
  let component: SourceChart;
  let fixture: ComponentFixture<SourceChart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SourceChart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SourceChart);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
