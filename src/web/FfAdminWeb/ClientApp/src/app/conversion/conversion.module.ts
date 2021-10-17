import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { ConversionDayComponent, SelectOptionComponent, LiquidationComponent } from './conversionday.component';

@NgModule({
  declarations: [
    ConversionDayComponent,
    SelectOptionComponent,
    LiquidationComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class ConversionModule {

}