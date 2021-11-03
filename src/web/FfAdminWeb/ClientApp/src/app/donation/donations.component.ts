import { Component } from '@angular/core';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption, IEventNewOption, IValidationMessage, IEventStatistics, IFullEvent, IDonationsByCurrency } from '../interfaces/interfaces';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { ErrorDialog } from '../dialogs/error.dialog';
import { InfoDialog } from '../dialogs/info.dialog';

@Component({
  selector: 'ff-donations',
  templateUrl: './donations.component.html'
})
export class DonationsComponent {
}

@Component({
  selector: 'ff-donations-grid',
  templateUrl: './donationsgrid.component.html'
})

export class DonationsGridComponent {
  constructor(private admin: Admin) {
    this.fetchData();
  }
  public data: IDonationsByCurrency[];
  public displayedColumns = ["currency", "amount", "worth", "allocated", "transferred"];
  public async fetchData() {
    this.data = await this.admin.getDonationsByCurrency();
  }
}