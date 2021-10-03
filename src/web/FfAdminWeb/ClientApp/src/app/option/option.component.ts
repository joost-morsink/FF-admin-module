import { Component, Output } from '@angular/core';
import { Admin } from '../admin/admin';
import { EventStore } from '../eventstore/eventstore';
import { IOption, IEventNewOption, IValidationMessage } from '../interfaces/interfaces';
import { } from '@angular/material';
import { EventEmitter } from 'protractor';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';

@Component({
  selector: 'ff-options',
  templateUrl: './options.component.html'
})
export class OptionsComponent {
  constructor(private admin: Admin, private eventStore: EventStore) {
    this.fetchOptions();
  }
  public async fetchOptions(): Promise<void> {
    let opts = await this.admin.getOptions();
    this.data = opts;
  }

  public data: IOption[] = null;
  public displayedColumns: string[] = ["code", "name", "currency", "fractions", "badyear", "update"];
}

@Component({
  selector: 'ff-add-option',
  templateUrl: './addOption.component.html'
})
export class AddOptionComponent {
  constructor(private eventStore: EventStore) {
    this.newOption();
  }
  public showAddForm: boolean = false;
  public adding: boolean = false;

  private newOption() {
    this.code = new FormControl("");
    this.name = new FormControl("");
    this.currency = new FormControl("");
    this.reinvestment_fraction = new FormControl("0.45");
    this.futureFund_fraction = new FormControl("0.1");
    this.charity_fraction = new FormControl("0.45");
    this.bad_year_fraction = new FormControl("0.01");
    this.formGroup = new FormGroup({
      code: this.code,
      name: this.name,
      currency: this.currency,
      reinvestment_fraction: this.reinvestment_fraction,
      futureFund_fraction: this.futureFund_fraction,
      charity_fraction: this.charity_fraction,
      bad_year_fraction: this.bad_year_fraction
    });
  }
  public formGroup: FormGroup;
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
    } finally {
      this.adding = false;
    }
  }
}