import { Routes } from '@angular/router';
import { TimesheetComponent } from './components/timesheet/timesheet.component';

export const routes: Routes = [
  { path: '', redirectTo: '/timesheet', pathMatch: 'full' },
  { path: 'timesheet', component: TimesheetComponent },
];
