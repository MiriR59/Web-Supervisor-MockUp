import { Component } from '@angular/core';
import { NgIf, NgFor, AsyncPipe } from '@angular/common';
import { Observable, of } from 'rxjs';
import { catchError, shareReplay } from 'rxjs';

import { SourcesService } from '../services/sources-service';
import { SourceDto } from '../models/source-dto';
import { SourceChartComponent } from '../source-chart-component/source-chart-component';

@Component({
  selector: 'app-dashboard-component',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, SourceChartComponent],
  templateUrl: './dashboard-component.html',
  styleUrl: './dashboard-component.css',
})
export class DashboardComponent {
  sources$!: Observable<SourceDto[]>;
  errorMessage: string | null = null;

  // sources$ moved to the constructor so that this.sourceService get injected on time
  constructor(private sourcesService: SourcesService) {
    this.sources$ = this.sourcesService.getAll().pipe(
      shareReplay(1),
      catchError((err) => {
        console.error('Failed to load sources', err);
        this.errorMessage = 'Failed to load sources.';
        return of([] as SourceDto[]);
      })
    )
  }
}