import { Component, Input, signal } from '@angular/core';
import { NgIf, NgFor, AsyncPipe, DatePipe, DecimalPipe } from '@angular/common';
import { BehaviorSubject, Observable, of, combineLatest, Subject } from 'rxjs';
import { catchError, shareReplay, switchMap, exhaustMap, startWith, tap, map, finalize, merge } from 'rxjs';
import { NgxEchartsDirective } from 'ngx-echarts';

import { SourceDto } from '../models/source-dto'; 
import { ReadingDto } from '../models/reading-dto';
import { ReadingsService } from '../services/readings-service';
import { RefreshService } from '../services/refresh-service';
import { SourcesService } from '../services/sources-service';
import { AuthService } from '../services/auth-service';

type TimeWindow = '5m' | '15m' | 'all';
type XY = [number, number];
type Points = {
  rpm: XY[];
  power: XY[];
  temp: XY[];
};
import type { EChartsOption } from 'echarts';
import { LagDto } from '../models/lag-dto';

@Component({
  selector: 'app-source-chart-component',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, DatePipe, DecimalPipe, NgxEchartsDirective],
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

  lag$!: Observable<LagDto>;

  points$!: Observable<Points>;
  rpmOptions$!: Observable<EChartsOption>;
  powerOptions$!: Observable<EChartsOption>;
  tempOptions$!: Observable<EChartsOption>;

  constructor(
    private readingService: ReadingsService,
    private refreshService: RefreshService,
    private sourcesService: SourcesService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    const reload$ = merge(
      of(void 0),
      this.refreshService.chartRefresh$,
    );
    
    this.readings$ = combineLatest([this.window$, reload$]).pipe(
      switchMap(([w, _]) => {
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

        const request$ = this.authService.isLoggedIn()
            ? this.readingService.getHistoryForSource(this.source.id, from, to)
            : this.readingService.getHistoryForPublicSource(this.source.id, from, to);

        return request$.pipe(
          catchError((err) => {
            console.error('Failed to load readings', err);
            return of([] as ReadingDto[]);
          })
        )       
      }),
      shareReplay({ bufferSize: 1, refCount: true}),
    );

    this.lag$ = reload$.pipe(
      switchMap(() => this.readingService.getLagForSource(this.source.id)),
      catchError((err) => {
        console.error('Failed to load lag', err);
        return of(null as any);
      }),
      shareReplay({ bufferSize: 1, refCount: true})
    );

    //Setup points for plotting
    this.points$ = this.readings$.pipe(
      map(readings => [...readings].sort(
        (a,b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
      )),
      map(sorted => ({
        rpm: sorted.map(r => [new Date(r.timestamp).getTime(), r.rpm] as XY),
        power: sorted.map(r => [new Date(r.timestamp).getTime(), r.power] as XY),
        temp: sorted.map(r => [new Date(r.timestamp).getTime(), r.temperature] as XY)
      })),
      shareReplay({ bufferSize: 1, refCount: true})
    );

    this.rpmOptions$ = this.points$.pipe(
      map(p => this.buildOptions('RPM', p.rpm, 'rpm'))
    );

    this.powerOptions$ = this.points$.pipe(
      map(p => this.buildOptions('Power', p.power, 'kW'))
    );

    this.tempOptions$ = this.points$.pipe(
      map(p => this.buildOptions('Temperature', p.temp, '°C'))
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
            this.refreshService.requestDashboardRefreshNow();
          }),

          finalize(() => this.isApplying.set(false))
        )
      ),

      startWith(this.source.isEnabled),
      shareReplay({ bufferSize: 1, refCount: true})
    );
  }

  setWindow(w: TimeWindow) {
    this.currentWindow.set(w);
    this.window$.next(w);
  }

  toggleEnabled(current: boolean) {
    this.setEnabled$.next(!current);
  }

  private buildOptions(title: string, data: XY[], unit?: string): EChartsOption {
    return {
      tooltip: { trigger: 'axis' },
      grid: { left: 45, right: 15, top: 35, bottom: 35 },
      xAxis: {
        type: 'time',
        name: 'Time',
        nameLocation: 'middle',
        nameGap: 30,
        axisLabel: {
          formatter: (value: number) => {
            const d = new Date(value);
            return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit'});
          },

          hideOverlap: true,
        }
      },
      yAxis: {
        type: 'value',
        scale: true,
        name: unit ? `${title} [${unit}]` : title,
        nameLocation: 'end',
      },
      series: [{
        type: 'line',
        showSymbol: false,
        data,
      }],
    };
  }
}
