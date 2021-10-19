import { Component, EventEmitter, Input, Output, OnInit, Injectable } from '@angular/core';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption, IEvent } from '../interfaces/interfaces';
import { ErrorDialog } from '../error/error.dialog';

type ProcessStep = 'init' | 'liquidate' | 'exit' | 'transfer' | 'enter' | 'invest';

@Component({
  selector: 'ff-conversion-day-component',
  templateUrl: './conversionday.component.html'
})
export class ConversionDayComponent {
  constructor(private admin: Admin) { }
  public option: IOption;
  public step: string = 'init';
  public onOptionSelected(option: { option: IOption, process: ProcessStep }) {
    this.option = option.option;
    this.step = option.process;
  }
  public async onLiquidated(dummy: any) {
    this.option = await this.admin.getOption(this.option.id);
    this.step = 'exit';
  }
  public onExited(dummy: any) {
    this.step = 'transfer';
  }
  public onTransferred(dummy: any) {
    this.step = 'enter';
  }
  public onEntered(dummy: any) {
    this.step = 'invest';
  }
  public onInvested(dummy: any) {
    this.option = null;
    this.step = 'init';
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
  public displayedColumns: string[] = ["name", "liquidate", "enter", "transfer"];
  public options: IOption[];
  @Output() public optionSelected: EventEmitter<{ option: IOption, process: ProcessStep }> = new EventEmitter();

  public click(option: IOption, process: ProcessStep) {
    this.optionSelected.emit({ option: option, process: process });
  }
  public async fetchOptions() {
    this.options = await this.admin.getOptions();
  }
}

export abstract class ConversionBaseComponent {
  constructor(protected eventStore: EventStore, protected dialog: MatDialog) { }

  public enabled: boolean;

  public async importAndProcess(event: IEvent, success?: EventEmitter<void>) {
    try {
      this.enabled = false;
      await this.eventStore.postEvent(event);
      await this.eventStore.process();
      success?.emit();
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
      this.enabled = true;
    }
  }
}

@Component({
  selector: 'ff-liquidation-admin',
  templateUrl: './liquidation.component.html'
})
export class LiquidationComponent extends ConversionBaseComponent implements OnInit {
  constructor(private admin: Admin, eventStore: EventStore, dialog: MatDialog) {
    super(eventStore, dialog);
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
    this.enabled = true;
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
    await this.importAndProcess(event, this.liquidated);
  }
}

@Component({
  selector: 'ff-exit-admin',
  templateUrl: './exit.component.html'
})
export class ExitComponent extends ConversionBaseComponent implements OnInit{
  constructor(eventStore: EventStore, private admin: Admin, dialog: MatDialog) {
    super(eventStore, dialog);
  }
  @Input() public option: IOption;
  @Output() public exited: EventEmitter<void> = new EventEmitter();

  public exit_amount: number;
  public timestamp: FormControl;
  public exitAmount: FormControl;
  public formGroup: FormGroup;

  public ngOnInit() {
    this.timestamp = new FormControl(new Date().toISOString());
    this.exitAmount = new FormControl("0.00");
    this.formGroup = new FormGroup({
      timestamp: this.timestamp,
      exitAmount: this.exitAmount
    });
  }
  public async recalculate() {
    this.exit_amount = await this.admin.calculateExit(this.option, this.option.invested_amount, this.timestamp.value);
  }
  public async exit() {
    if (parseFloat(this.exitAmount.value) == 0) {
      this.exited.emit();
      return;
    }
    let event = {
      type: 'CONV_EXIT',
      timestamp: this.timestamp.value,
      option: this.option.code,
      exit_amount: this.exitAmount.value,
    }
    await this.importAndProcess(event, this.exited);
  }
}