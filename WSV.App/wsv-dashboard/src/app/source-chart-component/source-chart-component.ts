import { Component, Input } from '@angular/core';
import { NgIf, NgFor, AsyncPipe } from '@angular/common';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, shareReplay, switchMap } from 'rxjs';

import { SourceDto } from '../models/source-dto'; 
import { ReadingDto } from '../models/reading-dto';
import { ReadingsService } from '../services/readings-service';

type TimeWindow = '5m' | '15m' | 'all';

@Component({
  selector: 'app-source-chart-component',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe],
  templateUrl: './source-chart-component.html',
  styleUrl: './source-chart-component.css',
})

export class SourceChartComponent {
  // Comes from the dashboard
  @Input() source!: SourceDto;

  readings$!: Observable<ReadingDto[]>;

  private window$ = new BehaviorSubject<TimeWindow>('all');
  currentWindow: TimeWindow = 'all';

  constructor(private readingService: ReadingsService) {}

  ngOnInit(): void {
    // Fetch readings from BE
    this.readings$ = this.window$.pipe(
      switchMap((w) => {
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
  }

  setWindow(w: TimeWindow) {
    this.currentWindow = w;
    this.window$.next(w);
  }
}
