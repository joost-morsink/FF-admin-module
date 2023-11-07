import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { AuditComponent, ExportComponent } from './export.component';

@NgModule({
  declarations: [
    AuditComponent,
    ExportComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class ExportModule {
}
