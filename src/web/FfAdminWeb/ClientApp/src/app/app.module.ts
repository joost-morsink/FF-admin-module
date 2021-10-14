import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FfUiModule } from './ffUi.module';
import { FfBackendModule } from './backend/backend.module';

import { CharityModule } from './charity/charity.module';
import { CharitiesComponent } from './charity/charity.component';
import { EventModule } from './events/event.module';
import { EventsComponent } from './events/events.component';
import { ImportModule } from './import/import.module';
import { ImportCsvComponent } from './import/import.component';
import { OptionModule } from './option/option.module';
import { OptionsComponent } from './option/option.component';
import { SessionModule } from './session/session.module';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';

import { ErrorDialog } from './error/error.dialog';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    ErrorDialog,
  ],
  entryComponents: [
    ErrorDialog,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'charities', component: CharitiesComponent },
      { path: 'options', component: OptionsComponent },
      { path: 'events', component: EventsComponent },
      { path: 'donations', component: ImportCsvComponent }
    ], { relativeLinkResolution: 'legacy' }),
    FfBackendModule,
    FfUiModule,
    CharityModule,
    EventModule,
    ImportModule,
    OptionModule,
    SessionModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
