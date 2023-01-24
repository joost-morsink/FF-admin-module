import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { CharitiesComponent } from './charity.component';
import {CharityPartitionComponent} from "./charity_partition.component";
import {RouterModule} from "@angular/router";

@NgModule({
  declarations: [
    CharitiesComponent,
    CharityPartitionComponent
  ],
  imports: [
    CommonModule,
    FfUiModule,
    RouterModule
  ]
})
export class CharityModule {
}
