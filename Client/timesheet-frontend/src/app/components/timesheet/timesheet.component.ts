import { Component, OnInit } from '@angular/core';
import { TimesheetEntry, ExportParams } from '../../models/timesheet.model';
import { TimesheetService } from '../../services/timesheet.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  format,
  startOfWeek,
  endOfWeek,
  eachDayOfInterval,
  parseISO,
} from 'date-fns';

@Component({
  selector: 'app-timesheet',
  imports: [CommonModule, FormsModule],
  templateUrl: './timesheet.component.html',
  styleUrl: './timesheet.component.css',
})
export class TimesheetComponent implements OnInit {
  currentWeek: Date[] = [];
  weekEntries: Map<string, TimesheetEntry> = new Map();
  editingStates: Map<string, boolean> = new Map();
  selectedWeekStart: Date = startOfWeek(new Date(), { weekStartsOn: 0 });

  exportParams: ExportParams = {
    name: '',
    employeeId: '',
    location: '',
    department: '',
  };

  showExportDialog = false;
  loading = false;
  error: string | null = null;
  successMessage: string | null = null;

  constructor(private timesheetService: TimesheetService) {}

  ngOnInit() {
    this.loadCurrentWeek();
  }

  loadCurrentWeek() {
    this.currentWeek = eachDayOfInterval({
      start: this.selectedWeekStart,
      end: endOfWeek(this.selectedWeekStart, { weekStartsOn: 0 }),
    });

    const weekStartStr = format(this.selectedWeekStart, 'yyyy-MM-dd');
    this.loading = true;
    this.weekEntries.clear();
    this.editingStates.clear();

    this.timesheetService.getEntriesByWeek(weekStartStr).subscribe({
      next: (entries) => {
        entries.forEach((entry) => {
          this.weekEntries.set(entry.date, entry);
        });
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load entries';
        this.loading = false;
        console.log(err);
      },
    });
  }

  hasEntry(date: Date): boolean {
    const dateStr = format(date, 'yyyy-MM-dd');
    return this.weekEntries.has(dateStr);
  }

  getEntry(date: Date): TimesheetEntry | undefined {
    const dateStr = format(date, 'yyyy-MM-dd');
    return this.weekEntries.get(dateStr);
  }

  isEditing(date: Date): boolean {
    const dateStr = format(date, 'yyyy-MM-dd');
    return this.editingStates.get(dateStr) || false;
  }

  startEdit(date: Date) {
    const dateStr = format(date, 'yyyy-MM-dd');
    let entry = this.weekEntries.get(dateStr);
    if (!entry) {
      entry = {
        date: dateStr,
        startTime: '09:00',
        endTime: '17:00',
        odometerStart: undefined,
        odometerEnd: undefined,
      };
      this.weekEntries.set(dateStr, entry);
    }
    this.editingStates.set(dateStr, true);
  }

  cancelEdit(date: Date) {
    const dateStr = format(date, 'yyyy-MM-dd');
    const entry = this.weekEntries.get(dateStr);
    if (entry) {
      if (!entry.id) {
        this.weekEntries.delete(dateStr);
      }
      this.editingStates.delete(dateStr);
    }
  }

  saveEntry(date: Date) {
    const dateStr = format(date, 'yyyy-MM-dd');
    const entry = this.getEntry(date);
    if (!entry) return;

    this.loading = true;

    if (entry.id) {
      this.timesheetService.updateEntry(entry.id, entry).subscribe({
        next: () => {
          this.editingStates.set(dateStr, false);
          this.successMessage = 'Entry saved successfully';
          this.loading = false;
          setTimeout(() => (this.successMessage = null), 3000);
        },
        error: (err) => {
          this.error = 'Failed to save entry';
          this.loading = false;
        },
      });
    } else {
      this.timesheetService.addEntry(entry).subscribe({
        next: (savedEntry) => {
          this.weekEntries.set(savedEntry.date, savedEntry);
          this.editingStates.set(dateStr, false);
          this.successMessage = 'Entry saved successfully';
          this.loading = false;
          setTimeout(() => (this.successMessage = null), 3000);
        },
        error: (err) => {
          this.error = 'Failed to save entry';
          this.loading = false;
        },
      });
    }
  }

  deleteEntry(date: Date) {
    const dateStr = format(date, 'yyyy-MM-dd');
    const entry = this.weekEntries.get(dateStr);

    if (entry?.id) {
      this.loading = true;
      this.timesheetService.deleteEntry(entry.id).subscribe({
        next: () => {
          this.weekEntries.delete(dateStr);
          this.editingStates.delete(dateStr);
          this.successMessage = 'Entry deleted successfully';
          this.loading = false;
          setTimeout(() => (this.successMessage = null), 3000);
        },
        error: (err) => {
          this.error = 'Failed to delete entry';
          this.loading = false;
        },
      });
    }
  }

  previousWeek() {
    this.selectedWeekStart = new Date(
      this.selectedWeekStart.setDate(this.selectedWeekStart.getDate() - 7)
    );
    this.loadCurrentWeek();
  }

  nextWeek() {
    this.selectedWeekStart = new Date(
      this.selectedWeekStart.setDate(this.selectedWeekStart.getDate() + 7)
    );
    this.loadCurrentWeek();
  }

  openExportDialog() {
    this.showExportDialog = true;
  }

  exportTimesheet() {
    if (
      !this.exportParams.name ||
      !this.exportParams.employeeId ||
      !this.exportParams.location ||
      !this.exportParams.department
    ) {
      this.error = 'Please fill all export fields';
      return;
    }

    const weekStartStr = format(this.selectedWeekStart, 'yyyy-MM-dd');
    this.loading = true;
    this.timesheetService.exportCurrentWeek(this.exportParams).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `TimeEntries_Week_${format(
          this.selectedWeekStart,
          'yyyyMMdd'
        )}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.showExportDialog = false;
        this.loading = false;
        this.successMessage = 'Timesheet exported successfully';
        setTimeout(() => (this.successMessage = null), 3000);
      },
      error: (err) => {
        this.error = 'Failed to export timesheet';
        this.loading = false;
      },
    });
  }

  calculateHours(entry: TimesheetEntry): number {
    if (!entry.startTime || !entry.endTime) return 0;

    const start = parseISO(`2000-01-01T${entry.startTime}`);
    const end = parseISO(`2000-01-01T${entry.endTime}`);
    const diff = (end.getTime() - start.getTime()) / (1000 * 60 * 60);

    return Math.max(0, diff);
  }

  calculateKilometres(entry: TimesheetEntry): number | null {
    if (entry.odometerStart !== undefined && entry.odometerEnd !== undefined) {
      return entry.odometerEnd - entry.odometerStart;
    }
    return null;
  }
}
