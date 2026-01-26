import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ReadingDto } from '../models/reading-dto';

@Injectable({
  providedIn: 'root',
})
export class ReadingsService {
  constructor(private http: HttpClient) {}

  getHistoryForSource(
    sourceId: number,
    from?: string,
    to?: string
  ): Observable<ReadingDto[]> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);

    return this.http.get<ReadingDto[]>(
      `/api/readings/source/${sourceId}`,
      { params }
    );
  }
  
  getHistoryForPublicSource(
    sourceId: number,
    from?: string,
    to?: string
  ): Observable<ReadingDto[]> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);

    return this.http.get<ReadingDto[]>(
      `/api/readings/public/source/${sourceId}`,
      { params }
    );
  }
}
