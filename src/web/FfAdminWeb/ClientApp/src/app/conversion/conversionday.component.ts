import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption } from '../interfaces/interfaces';
import { ErrorDialog } from '../error/error.dialog';

@Component({
  selector: 'ff-conversion-day-component',
  templateUrl: './conversionday.component.html'
})
export class ConversionDayComponent {
  public option: IOption;
  public step: string = 'init';
  public onOptionSelected(option: IOption) {
    this.option = option;
    this.step = 'liquidate';
  }
  public onLiquidated(dummy: any) {
    this.step = 'exit';
  }
}

@Component({
  selector: 'ff-select-option',
  templateUrl: './selectOption.component.html'
})
export class SelectOptionComponent {
  constructor(private admin:Admin){
    this.fetchOptions();
  }
  
  public options : IOption[];
  @Output() public optionSelected : EventEmitter<IOption> = new EventEmitter();

  public click(option: IOption) {
    this.optionSelected.emit(option);
  }
  public async fetchOptions() {
    this.options = await this.admin.getOptions();
  }
}

@Component({
  selector: 'ff-liquidation-admin',
  templateUrl: './liquidation.component.html'
})
export class LiquidationComponent implements OnInit {
  constructor(private admin: Admin, private eventStore: EventStore, private dialog: MatDialog) {
    
  }
  @Input() public option: IOption;
  @Output() public liquidated: EventEmitter<void> = new EventEmitter();

  public exit_amount: number;

  public invested: FormControl;
  public timestamp: FormControl;
  public newInvested: FormControl;
  public newCash: FormControl;
  public transactionRef: FormControl;
  public formGroup: FormGroup;

  public ngOnInit(): void {
    this.invested = new FormControl(this.option.invested_amount?.toString());
    this.timestamp = new FormControl(new Date().toISOString());
    this.newInvested = new FormControl("0.00");
    this.newCash = new FormControl("0.00");
    this.transactionRef = new FormControl("");
    this.formGroup = new FormGroup({
      invested: this.invested,
      timestamp: this.timestamp,
      newInvested: this.newInvested,
      newCash: this.newCash,
      transactionRef: this.transactionRef
    });
  }

  public async recalculate() {
    this.exit_amount = await this.admin.calculateExit(this.option, parseFloat(this.invested.value), this.timestamp.value);
  }

  public async liquidate() {
    let event = {
      type: 'CONV_LIQUIDATE',
      timestamp: this.timestamp.value,
      option: this.option.code,
      invested_amount: this.newInvested.value,
      cash_amount: this.newCash.value,
      transaction_reference: this.transactionRef.value
    }
    try {
      await this.eventStore.postEvent(event);
      this.liquidated.emit();
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

    }
  }
}