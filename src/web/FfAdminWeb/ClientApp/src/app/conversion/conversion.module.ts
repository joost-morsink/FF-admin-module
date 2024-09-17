import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import {
  ConversionDayComponent,
  SelectOptionComponent,
  LiquidationComponent,
  ExitComponent,
  TransfersComponent,
  TransferComponent,
  EnterComponent,
  InvestComponent,
  InflationComponent
} from './conversionday.component';

@NgModule({
  declarations: [
    ConversionDayComponent,
    SelectOptionComponent,
    LiquidationComponent,
    ExitComponent,
    TransfersComponent,
    TransferComponent,
    EnterComponent,
    InvestComponent,
    InflationComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class ConversionModule {

}
