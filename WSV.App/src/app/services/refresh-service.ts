import { Injectable, effect } from '@angular/core';
import { Observable, Subject, timer, withLatestFrom, startWith, scan } from 'rxjs';

import { AuthService } from './auth-service';

@Injectable({
  providedIn: 'root',
})

export class RefreshService {  
  private readonly chartTick = timer(0, 10_000);

  private readonly chartRefresh = new Subject<void>();
  readonly chartRefresh$ = this.chartRefresh.asObservable();

  private readonly authRefresh = new Subject<void>();
  readonly authRefresh$ = this.authRefresh.asObservable();

  constructor(private authService: AuthService) {
    this.chartTick.subscribe(() => {
      this.chartRefresh.next();
    })

    effect(() => {
      this.authService.isLoggedIn();
      this.authRefresh.next();
      this.chartRefresh.next();
    })
  }

  requestDashboardRefreshNow() {
    this.authRefresh.next();
  }
  
  
}
