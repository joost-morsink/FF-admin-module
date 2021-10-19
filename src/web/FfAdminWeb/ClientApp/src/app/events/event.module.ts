import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { EventsComponent, EventStatsComponent, EventTileComponent, EventUnimportedComponent } from './events.component';

@NgModule({
  declarations: [
    EventsComponent,
    EventStatsComponent,
    EventTileComponent,
    EventUnimportedComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class EventModule {
}