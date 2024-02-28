import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import {EventListComponent, EventsComponent, EventStatsComponent, EventTileComponent} from './events.component';
import {ImportEventsComponent} from "./importEvents.component";

@NgModule({
  declarations: [
    EventsComponent,
    EventStatsComponent,
    EventTileComponent,
    EventListComponent,
    ImportEventsComponent
  ],
  imports: [
    CommonModule,
    FfUiModule
  ]
})
export class EventModule {
}
