import {Component, ChangeDetectionStrategy, ChangeDetectorRef} from '@angular/core';
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
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './donations.component.html'
})
export class DonationsComponent {
}

@Component({
  selector: 'ff-donations-grid',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './donationsgrid.component.html'
})

export class DonationsGridComponent {
  constructor(private admin: Admin, private cdr: ChangeDetectorRef) {
    this.fetchData();
  }
  public data: IDonationsByCurrency[] = [];
  public displayedColumns = ["currency", "amount", "worth", "allocated", "transferred"];
  public async fetchData() {
    this.data = await this.admin.getDonationsByCurrency();
    this.cdr.detectChanges();
  }
}
