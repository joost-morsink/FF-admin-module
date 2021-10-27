import { Component } from '@angular/core';
import { Admin } from '../backend/admin';
import { IAuditInfo } from '../interfaces/interfaces';

@Component({
  selector: 'ff-audit',
  templateUrl: './audit.component.html'
})
export class AuditComponent {
  constructor(private admin: Admin) {
    this.fetchAuditReports();
  }
  public async fetchAuditReports(): Promise<void> {
    let opts = await this.admin.getAuditReports();
    this.data = opts;
  }
  public data: IAuditInfo[] = null;
  public displayedColumns: string[] = ["hashcode"]
}
