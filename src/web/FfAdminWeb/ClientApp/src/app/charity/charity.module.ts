import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { CharitiesComponent } from './charity.component';

@NgModule({
  declarations: [
    CharitiesComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class CharityModule {
}