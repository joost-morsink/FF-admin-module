import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';
import {RouterModule} from '@angular/router';
import {FfUiModule} from './ffUi.module';
import {FfBackendModule} from './backend/backend.module';

import {CharityModule} from './charity/charity.module';
import {CharitiesComponent} from './charity/charity.component';
import {EventModule} from './events/event.module';
import {EventsComponent} from './events/events.component';
import {DonationModule} from './donation/donation.module';
import {DonationsComponent} from './donation/donations.component';
import {OptionModule} from './option/option.module';
import {OptionsComponent} from './option/option.component';
import {SessionModule} from './session/session.module';
import {ConversionModule} from './conversion/conversion.module';
import {ConversionDayComponent} from './conversion/conversionday.component';
import {DialogsModule} from './dialogs/dialogs.module';
import {ExportModule} from './export/export.module';
import {ExportComponent} from './export/export.component';

import {AppComponent} from './app.component';
import {NavMenuComponent} from './nav-menu/nav-menu.component';
import {HomeComponent} from './home/home.component';
import {CharityPartitionComponent} from "./charity/charity_partition.component";
import {CurrentBranch} from "./currentBranch";
import {BranchInterceptor} from "./branch-interceptor";
import {HTTP_INTERCEPTORS} from "@angular/common/http";

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'charities', component: CharitiesComponent },
      { path: 'partition/:id', component: CharityPartitionComponent },
      { path: 'options', component: OptionsComponent },
      { path: 'events', component: EventsComponent },
      { path: 'donations', component: DonationsComponent },
      { path: 'conversion', component: ConversionDayComponent },
      { path: 'export', component: ExportComponent }
    ], {}),
    FfBackendModule,
    FfUiModule,
    DialogsModule,
    CharityModule,
    EventModule,
    DonationModule,
    OptionModule,
    SessionModule,
    ConversionModule,
    ExportModule
  ],
  providers: [CurrentBranch,
    { provide: HTTP_INTERCEPTORS, useClass: BranchInterceptor, multi: true }],
  bootstrap: [AppComponent]
})
export class AppModule { }

