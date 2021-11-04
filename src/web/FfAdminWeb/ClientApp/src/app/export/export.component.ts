import { Component } from '@angular/core';
import { Admin } from '../backend/admin';
import { IAuditInfo } from '../interfaces/interfaces';

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
  constructor(private admin: Admin) {
    this.fetchAuditReports();
  }
  public async fetchAuditReports(): Promise<void> {
    let opts = await this.admin.getAuditReports();
    this.data = opts;
  }
  public data: IAuditInfo[] = null;
  public displayedColumns: string[] = ["hashcode", "timestamp"]
}

@Component({
  selector: 'ff-web-export',
  templateUrl: './webexport.component.html'
})
export class WebExportComponent {
  constructor() { }
  public downloadJson() {
    this.download('json');
  }
  public downloadCsv() {
    this.download('csv');
  }
  public download(format: string) {
    const link = document.createElement('a');
    link.setAttribute('href', `admin/export/${format}`);
    link.setAttribute('style', 'display:none;');
    link.setAttribute('download', `web_export.${format}`);
    document.body.appendChild(link);
    link.click();
    link.remove();
  }
}