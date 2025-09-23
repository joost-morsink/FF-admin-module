import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { DonationsComponent, DonationsGridComponent } from './donations.component';

@NgModule({
  declarations: [
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
