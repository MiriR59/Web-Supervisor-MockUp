import { Component } from '@angular/core';
import { NgIf, NgFor, AsyncPipe } from '@angular/common';
import { exhaustMap, Observable, of, merge } from 'rxjs';
import { catchError, shareReplay } from 'rxjs';

import { SourcesService } from '../services/sources-service';
import { SourceDto } from '../models/source-dto';
import { SourceChartComponent } from '../source-chart-component/source-chart-component';
import { RefreshService } from '../services/refresh-service';
import { AuthService } from '../services/auth-service';
import { LoginComponent } from "../login-component/login-component";

@Component({
  selector: 'app-dashboard-component',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, SourceChartComponent, LoginComponent],
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
    private authService: AuthService
  ) {
    this.sources$ = merge(
      of(void 0),
      this.refreshService.refreshRequest$).pipe(
        exhaustMap(() => {
          const sources$ = this.authService.isLoggedIn()
            ? this.sourcesService.getAll()
            : this.sourcesService.getPublicAll();

          return sources$.pipe(
            catchError((err) => {
              console.error('Failed to load sources', err);
              this.errorMessage = 'Failed to load sources';
              return of([] as SourceDto[]);
            })
          )
        }),
        shareReplay({ bufferSize: 1, refCount: true })
      );
  }

  trackBySourceId(index: number, source: SourceDto) {
    return source.id;
  }
}