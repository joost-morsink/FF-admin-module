import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { AuditComponent } from './audit.component';

@NgModule({
  declarations: [
    AuditComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class AuditModule {
}