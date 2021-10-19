import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { DatabaseComponent } from './database.component';

@NgModule({
  declarations: [
    DatabaseComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class DatabaseModule {
}