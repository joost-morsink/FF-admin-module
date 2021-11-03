import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { ImportCsvComponent } from './import.component';
import { DonationsComponent, DonationsGridComponent } from './donations.component';

@NgModule({
  declarations: [
    ImportCsvComponent,
    DonationsComponent,
    DonationsGridComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class DonationModule {
}