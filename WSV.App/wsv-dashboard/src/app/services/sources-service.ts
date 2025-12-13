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
}
