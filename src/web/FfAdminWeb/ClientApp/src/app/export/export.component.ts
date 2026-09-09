import {Component, ChangeDetectionStrategy, ChangeDetectorRef} from '@angular/core';
import { Admin } from '../backend/admin';
import { IAuditInfo } from '../interfaces/interfaces';
import {EventStore} from "../backend/eventstore";

@Component({
  selector: 'ff-export',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './export.component.html'
})
export class ExportComponent {

}

@Component({
  selector: 'ff-audit',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './audit.component.html'
})
export class AuditComponent {
  constructor(private admin: Admin, private eventStore: EventStore, private cdr: ChangeDetectorRef) {
    this.fetchAuditReports();
  }
  public async fetchAuditReports(): Promise<void> {
    let opts = await this.admin.getAuditReports();
    this.data = opts;
    this.cdr.detectChanges();
  }
  public data: IAuditInfo[] | null = null;
  public displayedColumns: string[] = ["hashCode", "timestamp"]
  public async audit() :Promise<void> {
    await this.eventStore.audit();
    this.fetchAuditReports();
  }
}
