export interface TimesheetEntry {
  id?: number;
  date: string;
  startTime: string;
  endTime: string;
  odometerStart?: number;
  odometerEnd?: number;
  employeeId?: number;
  hoursWorked?: number;
  kilometres?: number;
}

export interface Employee {
  id?: number;
  name: string;
  employeeId: string;
  location: string;
  department: string;
  timesheetEntries?: TimesheetEntry[];
}

export interface ExportParams {
  name: string;
  employeeId: string;
  location: string;
  department: string;
}
