import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { EventsComponent, EventStatsComponent, EventTileComponent } from './events.component';

@NgModule({
  declarations: [
    EventsComponent,
    EventStatsComponent,
    EventTileComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class EventModule {
}
