import { Component, signal } from '@angular/core';
import { NgIf, NgFor, AsyncPipe } from '@angular/common';
import { Observable, of, merge } from 'rxjs';
import { switchMap, catchError, shareReplay, distinctUntilChanged, mapTo } from 'rxjs';
import { toObservable } from '@angular/core/rxjs-interop';

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
  errorMessage = signal<string | null>(null);

  constructor(
    private sourcesService: SourcesService,
    private refreshService: RefreshService,
    private authService: AuthService

  ) {
    const reload$ = merge(
      of(void 0),
      this.refreshService.authRefresh$
    );
    
    this.sources$ = reload$.pipe(
      switchMap(() => {
        this.errorMessage.set(null);

        const request$ = this.authService.isLoggedIn()
          ? this.sourcesService.getAll()
          : this.sourcesService.getPublicAll();

        return request$.pipe(
          catchError((err) => {
            console.error('Failed to load sources', err);
            this.errorMessage.set('Failed to load sources');
            return of([] as SourceDto[]);
            })
        );
      }),
      shareReplay({ bufferSize: 1, refCount: true })
    );
  }

  trackBySourceId(index: number, source: SourceDto) {
    return source.id;
  }
}