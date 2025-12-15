import { Component } from '@angular/core';
import { NgIf, NgFor, AsyncPipe } from '@angular/common';
import { exhaustMap, Observable, of, merge } from 'rxjs';
import { catchError, shareReplay } from 'rxjs';

import { SourcesService } from '../services/sources-service';
import { SourceDto } from '../models/source-dto';
import { SourceChartComponent } from '../source-chart-component/source-chart-component';
import { RefreshService } from '../services/refresh-service';

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
  constructor(
    private sourcesService: SourcesService,
    private refreshService: RefreshService,
  ) {
    this.sources$ = merge(
      of(void 0),
      this.refreshService.refreshRequest$).pipe(
        exhaustMap(() => this.sourcesService.getAll().pipe(
          catchError(() => {
            console.error('Failed to load sources');
            this.errorMessage = 'Failed to load sources.';
            return of ([] as SourceDto[]);
          })
        )),
        shareReplay({ bufferSize: 1, refCount: true })
      );
  }

  trackBySourceId(index: number, source: SourceDto) {
    return source.id;
  }

}