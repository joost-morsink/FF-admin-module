import { Component } from '@angular/core';
import { Admin } from '../backend/admin';
import { IAuditInfo } from '../interfaces/interfaces';
import {EventStore} from "../backend/eventstore";

@Component({
  selector: 'ff-export',
  templateUrl: './export.component.html'
})
export class ExportComponent {

}

@Component({
  selector: 'ff-audit',
  templateUrl: './audit.component.html'
})
export class AuditComponent {
  constructor(private admin: Admin, private eventStore: EventStore) {
    this.fetchAuditReports();
  }
  public async fetchAuditReports(): Promise<void> {
    let opts = await this.admin.getAuditReports();
    this.data = opts;
  }
  public data: IAuditInfo[] = null;
  public displayedColumns: string[] = ["hashCode", "timestamp"]
  public async audit() :Promise<void> {
    await this.eventStore.audit();
    this.fetchAuditReports();
  }
}
