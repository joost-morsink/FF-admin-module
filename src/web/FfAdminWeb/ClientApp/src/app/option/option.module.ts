import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { OptionsComponent, AddOptionComponent, UpdateOptionDialog } from './option.component';

@NgModule({
  declarations: [
    OptionsComponent,
    AddOptionComponent,
    UpdateOptionDialog
  ],
  entryComponents: [
    UpdateOptionDialog
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class OptionModule {
}