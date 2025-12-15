import { Component, Input, signal } from '@angular/core';
import { NgIf, NgFor, AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';
import { BehaviorSubject, Observable, of, combineLatest, Subject } from 'rxjs';
import { catchError, shareReplay, switchMap, exhaustMap, take, skip, startWith, tap } from 'rxjs';

import { SourceDto } from '../models/source-dto'; 
import { ReadingDto } from '../models/reading-dto';
import { ReadingsService } from '../services/readings-service';
import { RefreshService } from '../services/refresh-service';
import { SourcesService } from '../services/sources-service';

type TimeWindow = '5m' | '15m' | 'all';

@Component({
  selector: 'app-source-chart-component',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, DatePipe, DecimalPipe],
  templateUrl: './source-chart-component.html',
  styleUrl: './source-chart-component.css',
})

export class SourceChartComponent {
  // Comes from the dashboard
  @Input() source!: SourceDto;

  readings$!: Observable<ReadingDto[]>;
  private window$ = new BehaviorSubject<TimeWindow>('all');
  currentWindow = signal<TimeWindow>('all');

  private setEnabled$ = new Subject<boolean>();
  enabled$!: Observable<boolean>;
  isApplying = signal(false);

  constructor(
    private readingService: ReadingsService,
    private refreshService: RefreshService,
    private sourcesService: SourcesService
  ) {}

  //Do I need to always refetch while tick or click happens and w emits? Cant I just compare something like _DbContext?

  ngOnInit(): void {
    // Fetch readings from BE
    this.readings$ = combineLatest([this.window$, this.refreshService.tick$]).pipe(
      switchMap(([w, _tick]) => {
        const now = new Date();
        let from: string | undefined;
        let to: string | undefined;

        if (w === '5m') {
          from = new Date(now.getTime() - 5 * 60 * 1000).toISOString();
          to = now.toISOString();
        }
        else if ( w === '15m') {
          from = new Date(now.getTime() - 15 * 60 * 1000).toISOString();
          to = now.toISOString();
        }

        return this.readingService.getHistoryForSource(this.source.id, from, to);
      }),
      shareReplay(1),
      catchError((err) => {
        console.error('Failed to load readings', err);
        return of([]);
        })
    );
    
    this.enabled$ = this.setEnabled$.pipe(
      tap(() => this.isApplying.set(true)),

      exhaustMap((newBool) =>
        this.sourcesService.setEnabled(this.source.id, newBool).pipe(
          catchError(() => {
            console.error('Failed to toggle machine');
            return of(!newBool);
          }),

          tap(() => {
            this.refreshService.markDirty(),
            this.refreshService.tick$.pipe(skip(1), take(1)).subscribe(() => {
              this.isApplying.set(false);
            });
          })
        )
      ),
      startWith(this.source.isEnabled),
      shareReplay(1)
    );
  }

  setWindow(w: TimeWindow) {
    this.currentWindow.set(w);
    this.window$.next(w);
  }

  toggleEnabled(current: boolean) {
    this.setEnabled$.next(!current);
  }
}
