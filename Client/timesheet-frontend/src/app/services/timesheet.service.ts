import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TimesheetEntry, ExportParams } from '../models/timesheet.model';

@Injectable({
  providedIn: 'root',
})
export class TimesheetService {
  private apiUrl =
    'https://timesheetpep-dnhhdhh6cbfaexf7.canadacentral-01.azurewebsites.net/api/TimeEntries';

  constructor(private http: HttpClient) {}

  getEntries(): Observable<TimesheetEntry[]> {
    return this.http.get<TimesheetEntry[]>(this.apiUrl);
  }

  getEntry(id: number): Observable<TimesheetEntry> {
    return this.http.get<TimesheetEntry>(`${this.apiUrl}/${id}`);
  }

  getEntriesByWeek(weekStart: string): Observable<TimesheetEntry[]> {
    return this.http.get<TimesheetEntry[]>(`${this.apiUrl}/week/${weekStart}`);
  }

  addEntry(entry: TimesheetEntry): Observable<TimesheetEntry> {
    return this.http.post<TimesheetEntry>(this.apiUrl, entry);
  }

  updateEntry(id: number, entry: TimesheetEntry): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, entry);
  }

  deleteEntry(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
  exportCurrentWeek(params: ExportParams): Observable<Blob> {
    const httpParams = new HttpParams()
      .set('name', params.name)
      .set('employeeId', params.employeeId)
      .set('location', params.location)
      .set('department', params.department);

    return this.http.get(`${this.apiUrl}/export/current-week`, {
      params: httpParams,
      responseType: 'blob',
    });
  }
}
