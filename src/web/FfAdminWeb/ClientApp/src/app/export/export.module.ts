import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { AuditComponent, ExportComponent, WebExportComponent } from './export.component';

@NgModule({
  declarations: [
    AuditComponent,
    ExportComponent,
    WebExportComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class ExportModule {
}