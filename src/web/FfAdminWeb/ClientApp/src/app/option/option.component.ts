import { Component, Output, Inject } from '@angular/core';
import { Admin } from '../admin/admin';
import { EventStore } from '../eventstore/eventstore';
import { IOption, IEventNewOption, IValidationMessage } from '../interfaces/interfaces';
import { } from '@angular/material';
import { EventEmitter } from 'protractor';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ErrorDialog } from '../error/error.dialog';

@Component({
  selector: 'ff-options',
  templateUrl: './options.component.html'
})
export class OptionsComponent {
  constructor(private admin: Admin, private eventStore: EventStore, private dialog: MatDialog) {
    this.fetchOptions();
  }
  public async fetchOptions(): Promise<void> {
    let opts = await this.admin.getOptions();
    this.data = opts;
  }
  public async update(row: IOption) {
    let dlg = this.dialog.open(UpdateOptionDialog, {
      data: { option: row }
    });
  }
  public data: IOption[] = null;
  public displayedColumns: string[] = ["code", "name", "currency", "fractions", "badyear", "update"];
}

@Component({
  selector: 'ff-add-option',
  templateUrl: './addOption.component.html'
})
export class AddOptionComponent {
  constructor(private eventStore: EventStore, private dialog: MatDialog) {
    this.newOption();
  }
  public showAddForm: boolean = false;
  public adding: boolean = false;

  private newOption() {
    this.timestamp = new FormControl("");
    this.code = new FormControl("");
    this.name = new FormControl("");
    this.currency = new FormControl("");
    this.reinvestment_fraction = new FormControl("0.45");
    this.futureFund_fraction = new FormControl("0.1");
    this.charity_fraction = new FormControl("0.45");
    this.bad_year_fraction = new FormControl("0.01");
    this.formGroup = new FormGroup({
      timestamp: this.timestamp,
      code: this.code,
      name: this.name,
      currency: this.currency,
      reinvestment_fraction: this.reinvestment_fraction,
      futureFund_fraction: this.futureFund_fraction,
      charity_fraction: this.charity_fraction,
      bad_year_fraction: this.bad_year_fraction
    });
    this.eventStore.getStatistics().then(stats => this.timestamp.setValue(stats.firstUnprocessed || stats.lastProcessed));
  }
  public formGroup: FormGroup;
  public timestamp: FormControl;
  public code: FormControl;
  public name: FormControl;
  public currency: FormControl;
  public reinvestment_fraction: FormControl;
  public futureFund_fraction: FormControl;
  public charity_fraction: FormControl;
  public bad_year_fraction: FormControl;

  public clickAddForm() {
    this.showAddForm = true;
    this.newOption();
  }

  public async addOption(): Promise<void> {
    this.adding = true;
    try {
      let e = {
        type: "META_NEW_OPTION",
        timestamp: this.timestamp.value,
        code: this.code.value,
        name: this.name.value,
        currency: this.currency.value,
        reinvestment_fraction: this.reinvestment_fraction.value,
        futureFund_fraction: this.futureFund_fraction.value,
        charity_fraction: this.charity_fraction.value,
        bad_year_fraction: this.bad_year_fraction.value
      };
      await this.eventStore.postEvent(e);
      this.showAddForm = false;
      this.newOption();
    } catch (ex) {
      for (let err of ex.error) {
        let key = err.key[0].toLowerCase() + err.key.substring(1);

        if (key in this) {
          let control: FormControl = this[key];
          let ve: ValidationErrors = {};
          ve["message"] = err.message;

          control.setErrors(ve);
        }
      }
      this.dialog.open(ErrorDialog, {
        data: { errors: ex.error },
      });
    } finally {
      this.adding = false;
    }
  }
}

@Component({
  selector: 'ff-update-option-dialog',
  templateUrl: './updateOption.dialog.html'
})
export class UpdateOptionDialog {
  constructor(private eventStore: EventStore,
    private dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: { option: IOption },
    private dialogRef: MatDialogRef<UpdateOptionDialog>)
  {
    this.initialize(data.option);
  }
 
  public updating: boolean = false;

  private initialize(option: IOption) {
    this.timestamp = new FormControl("");
    this.reinvestment_fraction = new FormControl(option.reinvestment_fraction);
    this.futureFund_fraction = new FormControl(option.futureFund_fraction);
    this.charity_fraction = new FormControl(option.charity_fraction);
    this.bad_year_fraction = new FormControl(option.bad_year_fraction);
    this.formGroup = new FormGroup({
      timestamp: this.timestamp,
      reinvestment_fraction: this.reinvestment_fraction,
      futureFund_fraction: this.futureFund_fraction,
      charity_fraction: this.charity_fraction,
      bad_year_fraction: this.bad_year_fraction
    });
    this.timestamp.setValue(new Date().toISOString());
  }
  public formGroup: FormGroup;
  public timestamp: FormControl;
  public code: FormControl;
  public name: FormControl;
  public currency: FormControl;
  public reinvestment_fraction: FormControl;
  public futureFund_fraction: FormControl;
  public charity_fraction: FormControl;
  public bad_year_fraction: FormControl;

  public async updateOption(): Promise<void> {
    this.updating = true;
    try {
      let e = {
        type: "META_UPDATE_FRACTIONS",
        timestamp: this.timestamp.value,
        code: this.data.option.code,
        reinvestment_fraction: this.reinvestment_fraction.value,
        futureFund_fraction: this.futureFund_fraction.value,
        charity_fraction: this.charity_fraction.value,
        bad_year_fraction: this.bad_year_fraction.value
      };
      await this.eventStore.postEvent(e);
      this.dialogRef.close();
    } catch (ex) {
      for (let err of ex.error) {
        let key = err.key[0].toLowerCase() + err.key.substring(1);

        if (key in this) {
          let control: FormControl = this[key];
          let ve: ValidationErrors = {};
          ve["message"] = err.message;

          control.setErrors(ve);
        }
      }
      this.dialog.open(ErrorDialog, {
        data: { errors: ex.error },
      });
    } finally {
      this.updating = false;
    }
  }
}