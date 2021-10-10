import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { MatTableModule } from "@angular/material/table";
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule } from '@angular/material/dialog';
import { EventStore } from './eventstore/eventstore';
import { Admin } from './admin/admin';


import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { SessionButtonComponent, CommitDialog } from './sessionButton/sessionButton.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { OptionsComponent, AddOptionComponent } from './option/option.component';
import { EventsComponent, EventStatsComponent, EventUnimportedComponent } from './events/events.component';
import { CharitiesComponent } from './charity/charity.component';
import { ErrorDialog } from './error/error.dialog';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    SessionButtonComponent,
    OptionsComponent,
    AddOptionComponent,
    CharitiesComponent,
    EventsComponent,
    EventStatsComponent,
    EventUnimportedComponent,
    ErrorDialog,
    CommitDialog
  ],
  entryComponents: [
    ErrorDialog,
    CommitDialog
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'charities', component: CharitiesComponent },
      { path: 'options', component: OptionsComponent },
      { path: 'events', component: EventsComponent }
    ], { relativeLinkResolution: 'legacy' }),
    BrowserAnimationsModule,
    MatTableModule,
    MatIconModule,
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatFormFieldModule,
    MatDialogModule
  ],
  providers: [EventStore, Admin],
  bootstrap: [AppComponent]
})
export class AppModule { }
