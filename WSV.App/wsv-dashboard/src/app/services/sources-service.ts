import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SourceDto } from '../models/source-dto';

@Injectable({
  providedIn: 'root',
})
export class SourcesService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<SourceDto[]> {
  return this.http.get<SourceDto[]>('/api/sources');
  }

  setEnabled(
    sourceId: number,
    isEnabled: boolean
  ): Observable<boolean> {
    return this.http.patch<boolean>(
      `/api/sources/${sourceId}/enabled`,
      { isEnabled }
    );
  }
}
