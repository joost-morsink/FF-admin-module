import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { ImportCsvComponent } from './import.component';

@NgModule({
  declarations: [
    ImportCsvComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class ImportModule {
}