import { Injectable, signal } from '@angular/core';
import { Observable, Subject, timer, withLatestFrom, startWith, scan } from 'rxjs';
import { shareReplay } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export class RefreshService {  
  // Emit immediately (0) and then every 10 secs
  // shareReplay makes this stream shared across all subs
  readonly tick$: Observable<number> = timer(0, 10_000).pipe(
    shareReplay({ bufferSize: 1, refCount: true})
  );

  private isDirty = false;
  private readonly refresh = new Subject<void>();
  // Separates internal refresh and Observable, so nothing but this service can emit
  readonly refreshRequest$ = this.refresh.asObservable();

  markDirty() {
    this.isDirty = true;
  }

  constructor() {
    this.tick$.subscribe(() => {
      if (this.isDirty == true) {
        this.isDirty = false;
        this.refresh.next();
      }
    });
  }
}
