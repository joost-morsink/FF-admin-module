import { Component, EventEmitter, Input, Output, OnInit, Injectable, ViewChildren, QueryList } from '@angular/core';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption, IEvent, IOpenTransfer } from '../interfaces/interfaces';
import { ErrorDialog } from '../dialogs/error.dialog';
import { InfoDialog } from "../dialogs/info.dialog";
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
  public async refreshOption() {
    this.option = await this.admin.getOption(this.option.id);
  }
  public async onLiquidated(dummy: any) {
    await this.refreshOption()
    this.step = 'exit';
  }
  public async onExited(dummy: any) {
    await this.refreshOption();
    this.step = 'transfer';
  }
  public onTransferred(dummy: any) {
    this.step = 'enter';
  }
  public async onEntered(dummy: any) {
    await this.refreshOption();
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
  public displayedColumns: string[] = ["name", "liquidate", "transfer", "enter"];
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

  public async importAndProcess<T>(event: IEvent, success?: EventEmitter<T>, data?: T) {
    try {
      this.enabled = false;
      await this.eventStore.postEvent(event);
      await this.eventStore.process();
      if (data)
        success?.emit(data);
      else
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
    this.enabled = true;
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
      amount: this.exitAmount.value,
    }
    await this.importAndProcess(event, this.exited);
  }
}
@Component({
  selector: 'ff-transfers-admin',
  templateUrl: './transfers.component.html'
})
export class TransfersComponent implements OnInit {
  constructor(private admin: Admin, private eventStore: EventStore, private dialog: MatDialog) {
    this.fetchOpenTransfers().then();
  }
  @Output() public done: EventEmitter<void> = new EventEmitter();
  public transfers: IOpenTransfer[];
  public file: File;
  public fileName: string;
  private fileInput: HTMLInputElement;
  public formGroup: FormGroup;
  public cutoff: FormControl;

  @ViewChildren('transfer') public transferComponents: QueryList<TransferComponent>;
  public ngOnInit() {
    this.cutoff = new FormControl("5.00")
    this.formGroup = new FormGroup({ cutoff: this.cutoff })
  }

  public async fetchOpenTransfers() {
    this.transfers = (await this.admin.getOpenTransfers()).sort((x, y) => x.name < y.name ? -1 : 1);
    var date = new Date().toISOString();
    setTimeout(() => {
      for (var t of this.transferComponents)
        t.timestamp.setValue(date);
    }, 0);
  }
  public onTransferCompleted(transfer: IOpenTransfer) {
    this.transfers = this.transfers.filter(t => t.charity != transfer.charity || t.currency != transfer.currency);
  }
  public onFileSelected(e) {
    this.fileInput = e.target;
    this.file = this.fileInput.files[0];
    this.fileName = this.file?.name;
  }
  public async executeUpload() {
    if(this.file) {
      try {
        await this.admin.importBankTransfers(this.file);
        this.fileInput.files = null;
        this.file = null;
        this.fileName = null;
        this.dialog.open(InfoDialog, {
          data: { title: "Success", message: "Import and processing successful!" }
        });
      } catch(ex) {
        this.dialog.open(ErrorDialog, {
          data: { errors: ex.error }
        }).afterClosed();
      }
    }
  }
  public downloadPain() {
    const link = document.createElement('a');
    link.setAttribute('href', `admin/charities/opentransfers/pain?currency=EUR&cutoff=${this.cutoff.value}`);
    link.setAttribute('style', 'display:none;');
    link.setAttribute('download', `charity_transfers.xml`);
    document.body.appendChild(link);
    link.click();
    link.remove();
  }
}

@Component({
  selector: 'ff-transfer',
  templateUrl: './transfer.component.html'
})
export class TransferComponent extends ConversionBaseComponent implements OnInit {
  constructor(eventStore: EventStore, dialog: MatDialog, private admin: Admin) {
    super(eventStore, dialog);
  }
  @Input() public transfer: IOpenTransfer;
  @Output() public completed: EventEmitter<IOpenTransfer> = new EventEmitter();

  public timestamp: FormControl;
  public amount: FormControl;
  public transactionRef: FormControl;
  public hasExchange: boolean;
  public exchangedAmount: FormControl;
  public exchangedCurrency: FormControl;
  public exchangeRef: FormControl;
  public formGroup: FormGroup;


  public ngOnInit() {
    this.timestamp = new FormControl();
    this.amount = new FormControl("0.00");
    this.transactionRef = new FormControl("");
    this.exchangedCurrency = new FormControl("EUR");
    this.exchangedAmount = new FormControl("0.00");
    this.exchangeRef = new FormControl("");
    this.formGroup = new FormGroup({
      timestamp: this.timestamp,
      amount: this.amount,
      transactionRef: this.transactionRef,
      exchangedAmount: this.exchangedAmount,
      exchangedCurrency: this.exchangedCurrency,
      exchangeRef: this.exchangeRef
    });
    this.enabled = true;
    this.hasExchange = false;
  }
  public setHasExchange(state: boolean) {
    this.hasExchange = state;
  }
  public totalAmount() {
    this.amount.setValue(Math.floor(this.transfer.amount * 100) / 100);
  }
  public async doTransfer() {
    let event = {
      type: "CONV_TRANSFER",
      timestamp: this.timestamp.value,
      charity: this.transfer.charity,
      currency: this.transfer.currency,
      amount: parseFloat(this.amount.value),
      transaction_reference: this.transactionRef.value,
      exchanged_currency: this.hasExchange ? this.exchangedCurrency.value : "",
      exchanged_amount: this.hasExchange ? this.exchangedAmount.value : null,
      exchange_reference: this.hasExchange ? this.exchangeRef.value : ""
    }
    await this.importAndProcess(event, this.completed, this.transfer);
  }
}

@Component({
  selector: 'ff-enter-admin',
  templateUrl: './enter.component.html'
})
export class EnterComponent extends ConversionBaseComponent implements OnInit {
  constructor(eventStore: EventStore, private admin: Admin, dialog: MatDialog) {
    super(eventStore, dialog);
  }
  @Input() public option: IOption;
  @Output() public entered: EventEmitter<void> = new EventEmitter();

  public timestamp: FormControl;
  public investedAmount: FormControl;
  public formGroup: FormGroup;

  public ngOnInit() {
    this.timestamp = new FormControl(new Date().toISOString());
    this.investedAmount = new FormControl("0.00");
    this.formGroup = new FormGroup({
      timestamp: this.timestamp,
      investedAmount: this.investedAmount
    });
    this.enabled = true;
  }
  public async enter() {
    let event = {
      type: 'CONV_ENTER',
      timestamp: this.timestamp.value,
      option: this.option.code,
      invested_amount: this.investedAmount.value,
    }
    await this.importAndProcess(event, this.entered);
  }
}

@Component({
  selector: 'ff-invest-admin',
  templateUrl: './invest.component.html'
})
export class InvestComponent extends ConversionBaseComponent implements OnInit {
  constructor(private admin: Admin, eventStore: EventStore, dialog: MatDialog) {
    super(eventStore, dialog);
  }
  @Input() public option: IOption;
  @Output() public invested: EventEmitter<void> = new EventEmitter();

  public timestamp: FormControl;
  public newInvested: FormControl;
  public newCash: FormControl;
  public transactionRef: FormControl;
  public formGroup: FormGroup;

  public ngOnInit(): void {
    this.timestamp = new FormControl(new Date().toISOString());
    this.newInvested = new FormControl(this.option.invested_amount);
    this.newCash = new FormControl(this.option.cash_amount);
    this.transactionRef = new FormControl("");
    this.formGroup = new FormGroup({
      timestamp: this.timestamp,
      newInvested: this.newInvested,
      newCash: this.newCash,
      transactionRef: this.transactionRef
    });
    this.enabled = true;
  }

  public async invest() {
    let event = {
      type: 'CONV_INVEST',
      timestamp: this.timestamp.value,
      option: this.option.code,
      invested_amount: this.newInvested.value,
      cash_amount: this.newCash.value,
      transaction_reference: this.transactionRef.value
    }
    await this.importAndProcess(event, this.invested);
  }
}
