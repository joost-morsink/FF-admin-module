import { NgModule } from '@angular/core';
import { CommonModule} from '@angular/common';
import { EventStore } from './eventstore';
import { Admin } from './admin';
import { HttpClientModule } from '@angular/common/http';

@NgModule({
  imports: [
    CommonModule,
    HttpClientModule
  ],
  providers: [
    EventStore,
    Admin
  ]
})
export class FfBackendModule { }