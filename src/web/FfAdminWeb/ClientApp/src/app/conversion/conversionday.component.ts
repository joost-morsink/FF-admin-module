import {
  Component, EventEmitter, Input, Output, OnInit, Injectable, ViewChildren, QueryList, ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption, IEvent, IOpenTransfer } from '../interfaces/interfaces';
import { ErrorDialog } from '../dialogs/error.dialog';
import { InfoDialog } from "../dialogs/info.dialog";
type ProcessStep = 'init' | 'liquidate' | 'exit' | 'transfer' | 'enter' | 'invest' | 'inflation' | 'price';

@Component({
  selector: 'ff-conversion-day-component',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './conversionday.component.html'
})
export class ConversionDayComponent {
  constructor(private admin: Admin, private cdr: ChangeDetectorRef) { }
  public option: IOption | null = null;
  public step: string = 'init';
  @Output() public defaultTimestamp : string = '';

  public onOptionSelected(option: { option: IOption, process: ProcessStep }) {
    this.option = option.option;
    this.step = option.process;
  }
  public async refreshOption() {
    if (!this.option) {
      throw new Error('No option selected.');
    }

    this.option = await this.admin.getOption(this.option.code);
  }
  public async onLiquidated(component: LiquidationComponent) {
    this.defaultTimestamp = component.timestamp.value;
    await this.refreshOption()
    this.step = 'exit';
    this.cdr.detectChanges();
  }
  public async onExited(dummy: any) {
    await this.refreshOption();
    this.step = 'transfer';
    this.cdr.detectChanges();
  }
  public onTransferred(dummy: any) {
    this.step = 'enter';
    this.cdr.detectChanges();
  }
  public async onEntered(component: EnterComponent) {
    this.defaultTimestamp = component.timestamp.value;
    await this.refreshOption();
    this.step = 'invest';
    this.cdr.detectChanges();
  }
  public onInvested(dummy: any) {
    this.option = null;
    this.step = 'init';
    this.cdr.detectChanges();
  }

  public onInflated(dummy: any) {
    this.option = null;
    this.step='init';
    this.cdr.detectChanges();
  }

  public onPriced(dummy: any) {
    this.option = null;
    this.step='init';
    this.cdr.detectChanges();
  }
}

@Component({
  selector: 'ff-select-option',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './selectOption.component.html'
})
export class SelectOptionComponent {
  constructor(private admin:Admin, private cdr: ChangeDetectorRef) {
    this.fetchOptions();
  }
  public displayedColumns: string[] = ["name", "liquidate", "transfer", "enter", "inflation", "price"];
  public options: IOption[] = [];
  @Output() public optionSelected: EventEmitter<{ option: IOption, process: ProcessStep }> = new EventEmitter();

  public click(option: IOption, process: ProcessStep) {
    this.optionSelected.emit({ option: option, process: process });
  }
  public async fetchOptions() {
    this.options = await this.admin.getOptions();
    this.cdr.detectChanges();
  }
}

export abstract class ConversionBaseComponent {
  constructor(protected eventStore: EventStore, protected dialog: MatDialog, protected cdr: ChangeDetectorRef) { }

  public enabled = true;

  public async importAndProcess<T>(event: IEvent, success?: EventEmitter<T>, data?: T) {
    try {
      this.enabled = false;
      await this.eventStore.postEvent(event);
      if (data)
        success?.emit(data);
      else
        success?.emit();
    } catch (ex: any) {
      const componentState = this as unknown as Record<string, unknown>;
      for (let err of ex.error) {
        let key = err.key[0].toLowerCase() + err.key.substring(1);
        let control = componentState[key];
        if (!(control instanceof UntypedFormControl)) {
          continue;
        }

        let ve: ValidationErrors = {};
        ve["message"] = err.message;
        control.setErrors(ve);
      }
      this.dialog.open(ErrorDialog, {
        data: { errors: ex.error },
      });
      this.enabled = true;
    }
    finally {
      this.cdr.detectChanges()
    }
  }
}

@Component({
  selector: 'ff-liquidation-admin',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './liquidation.component.html'
})
export class LiquidationComponent extends ConversionBaseComponent implements OnInit {
  constructor(private admin: Admin, eventStore: EventStore, dialog: MatDialog, cdr: ChangeDetectorRef) {
    super(eventStore, dialog, cdr);
  }
  @Input() public option!: IOption;
  @Output() public liquidated: EventEmitter<LiquidationComponent> = new EventEmitter<LiquidationComponent>();

  public exit_amount = 0;

  public compensation!: UntypedFormControl;
  public invested!: UntypedFormControl;
  public timestamp!: UntypedFormControl;
  public newInvested!: UntypedFormControl;
  public newCash!: UntypedFormControl;
  public transactionRef!: UntypedFormControl;
  public formGroup!: UntypedFormGroup;
  public loanable = 0;

  public ngOnInit(): void {
    this.compensation = new UntypedFormControl("0.00");
    this.invested = new UntypedFormControl(this.option.invested_amount?.toString());
    this.timestamp = new UntypedFormControl(new Date().toISOString());
    this.newInvested = new UntypedFormControl("0.00");
    this.newCash = new UntypedFormControl("0.00");
    this.transactionRef = new UntypedFormControl("");
    this.formGroup = new UntypedFormGroup({
      compensation: this.compensation,
      invested: this.invested,
      timestamp: this.timestamp,
      newInvested: this.newInvested,
      newCash: this.newCash,
      transactionRef: this.transactionRef
    });
    this.enabled = true;
  }
  public parseFloat(str:string) : number {
    return parseFloat(str);
  }
  public async recalculate() {
    this.loanable = await this.admin.getLoanableCash(this.option, new Date(this.timestamp.value));
    this.exit_amount = await this.admin.calculateExit(this.option, parseFloat(this.compensation.value), parseFloat(this.invested.value), this.timestamp.value);
    this.cdr.detectChanges();
  }
  public async increaseCash() {
    if(this.compensation.value > 0)
    {
      let event = {
        type: 'CONV_INCREASE_CASH',
        timestamp: this.timestamp.value,
        option: this.option.code,
        amount: this.compensation.value
      }
      await this.importAndProcess(event)
    }
  }
  public async liquidate() {
    await this.increaseCash();
    let event = {
      type: 'CONV_LIQUIDATE',
      timestamp: this.timestamp.value,
      option: this.option.code,
      invested_amount: this.newInvested.value,
      cash_amount: this.newCash.value,
      transaction_reference: this.transactionRef.value
    }
    await this.importAndProcess(event, this.liquidated, this);
  }
  public zeroLiquidation() {
    this.newInvested.setValue(this.invested.value);
    this.newCash.setValue((this.option.cash_amount ?? 0) + parseFloat(this.compensation.value));
  }
}

@Component({
  selector: 'ff-exit-admin',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './exit.component.html'
})
export class ExitComponent extends ConversionBaseComponent implements OnInit{
  constructor(eventStore: EventStore, private admin: Admin, dialog: MatDialog, cdr: ChangeDetectorRef) {
    super(eventStore, dialog, cdr);
  }
  @Input() public option!: IOption;
  @Input() public defaultTimestamp = '';
  @Output() public exited: EventEmitter<void> = new EventEmitter();

  public exit_amount = 0;
  public timestamp!: UntypedFormControl;
  public exitAmount!: UntypedFormControl;
  public formGroup!: UntypedFormGroup;
  public loanable = 0;

  public ngOnInit() {
    this.timestamp = new UntypedFormControl(this.defaultTimestamp || new Date().toISOString());
    this.exitAmount = new UntypedFormControl("0.00");
    this.formGroup = new UntypedFormGroup({
      timestamp: this.timestamp,
      exitAmount: this.exitAmount
    });
    this.enabled = true;
  }
  public async recalculate() {
    this.loanable = await this.admin.getLoanableCash(this.option, new Date(this.timestamp.value));
    this.exit_amount = await this.admin.calculateExit(this.option, 0, this.option.invested_amount, this.timestamp.value);
    this.exitAmount.setValue(this.exit_amount.toFixed(2));
    this.cdr.detectChanges();
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
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './transfers.component.html'
})
export class TransfersComponent implements OnInit {
  constructor(private admin: Admin, private eventStore: EventStore, private dialog: MatDialog) {
    this.fetchOpenTransfers().then();
  }
  @Output() public done: EventEmitter<void> = new EventEmitter();
  public transfers: IOpenTransfer[] = [];
  public file: File | null = null;
  public fileName: string | null = null;
  private fileInput: HTMLInputElement | null = null;
  public formGroup!: UntypedFormGroup;
  public cutoff!: UntypedFormControl;

  @ViewChildren('transfer') public transferComponents!: QueryList<TransferComponent>;
  public ngOnInit() {
    this.cutoff = new UntypedFormControl("5.00")
    this.formGroup = new UntypedFormGroup({ cutoff: this.cutoff })
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
  public onFileSelected(e: Event) {
    this.fileInput = e.target as HTMLInputElement;
    this.file = this.fileInput.files?.[0] ?? null;
    this.fileName = this.file?.name ?? null;
  }
  public async executeUpload() {
    if(this.file) {
      try {
        await this.admin.importBankTransfers(this.file);
        if (this.fileInput) {
          this.fileInput.value = '';
        }
        this.file = null;
        this.fileName = null;
        this.dialog.open(InfoDialog, {
          data: { title: "Success", message: "Import and processing successful!" }
        });
      } catch(ex: any) {
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
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './transfer.component.html'
})
export class TransferComponent extends ConversionBaseComponent implements OnInit {
  constructor(eventStore: EventStore, dialog: MatDialog, private admin: Admin, cdr: ChangeDetectorRef) {
    super(eventStore, dialog, cdr);
  }
  @Input() public transfer!: IOpenTransfer;
  @Output() public completed: EventEmitter<IOpenTransfer> = new EventEmitter();

  public timestamp!: UntypedFormControl;
  public amount!: UntypedFormControl;
  public transactionRef!: UntypedFormControl;
  public hasExchange = false;
  public exchangedAmount!: UntypedFormControl;
  public exchangedCurrency!: UntypedFormControl;
  public exchangeRef!: UntypedFormControl;
  public formGroup!: UntypedFormGroup;


  public ngOnInit() {
    this.timestamp = new UntypedFormControl();
    this.amount = new UntypedFormControl("0.00");
    this.transactionRef = new UntypedFormControl("");
    this.exchangedCurrency = new UntypedFormControl("EUR");
    this.exchangedAmount = new UntypedFormControl("0.00");
    this.exchangeRef = new UntypedFormControl("");
    this.formGroup = new UntypedFormGroup({
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
    this.cdr.detectChanges();
  }
}

@Component({
  selector: 'ff-enter-admin',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './enter.component.html'
})
export class EnterComponent extends ConversionBaseComponent implements OnInit {
  constructor(eventStore: EventStore, private admin: Admin, dialog: MatDialog, cdr: ChangeDetectorRef) {
    super(eventStore, dialog, cdr);
  }
  @Input() public option!: IOption;
  @Output() public entered: EventEmitter<EnterComponent> = new EventEmitter<EnterComponent>();

  public timestamp!: UntypedFormControl;
  public investedAmount!: UntypedFormControl;
  public formGroup!: UntypedFormGroup;

  public ngOnInit() {
    this.timestamp = new UntypedFormControl("");
    this.investedAmount = new UntypedFormControl("0.00");
    this.formGroup = new UntypedFormGroup({
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
    await this.importAndProcess(event, this.entered, this);
  }
}

@Component({
  selector: 'ff-invest-admin',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './invest.component.html'
})
export class InvestComponent extends ConversionBaseComponent implements OnInit {
  constructor(private admin: Admin, eventStore: EventStore, dialog: MatDialog, cdr: ChangeDetectorRef) {
    super(eventStore, dialog, cdr);
  }
  @Input() public option!: IOption;
  @Input() public defaultTimestamp = '';
  @Output() public invested: EventEmitter<void> = new EventEmitter();

  public transferInvestmentButtonDisabled = false;
  public timestamp!: UntypedFormControl;
  public newInvested!: UntypedFormControl;
  public newCash!: UntypedFormControl;
  public transactionRef!: UntypedFormControl;
  public formGroup!: UntypedFormGroup;
  public investment!: UntypedFormControl;
  public ngOnInit(): void {
    this.timestamp = new UntypedFormControl(this.defaultTimestamp);
    this.newInvested = new UntypedFormControl(this.option.invested_amount);
    this.newCash = new UntypedFormControl(this.option.cash_amount);
    this.investment = new UntypedFormControl("0");

    this.transactionRef = new UntypedFormControl("");
    this.formGroup = new UntypedFormGroup({
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
  public transferInvestment() {
    let transferAmount = Number(this.investment.value) || 0;
    this.newInvested.setValue((Number(this.newInvested.value) + transferAmount) || 0);
    this.newCash.setValue((Number(this.newCash.value) - transferAmount) || 0);
    this.transferInvestmentButtonDisabled = true;
  }
}

@Component({
  selector: 'ff-inflation-admin',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './inflation.component.html'
})
export class InflationComponent extends ConversionBaseComponent implements OnInit {
  constructor(private admin: Admin, eventStore: EventStore, dialog: MatDialog, cdr: ChangeDetectorRef) {
    super(eventStore, dialog, cdr);
  }
  @Input() public option!: IOption;
  @Output() public inflation: EventEmitter<void> = new EventEmitter();

  public timestamp!: UntypedFormControl;
  public invested!: UntypedFormControl;
  public inflationPercentage!: UntypedFormControl;
  public formGroup!: UntypedFormGroup;
  public ngOnInit(): void {
    this.timestamp = new UntypedFormControl(new Date().toISOString());
    this.invested = new UntypedFormControl(this.option.invested_amount);
    this.inflationPercentage = new UntypedFormControl("0");
    this.formGroup = new UntypedFormGroup({
      timestamp: this.timestamp,
      invested: this.invested,
      inflationPercentage: this.inflationPercentage
    });
    this.enabled = true;
  }

  public async inflate() {
    let event = {
      type: 'CONV_INFLATION',
      timestamp: this.timestamp.value,
      option: this.option.code,
      invested_amount: this.invested.value,
      inflation_factor: 1.0 + (Number(this.inflationPercentage.value) / 100.0)
    }
    await this.importAndProcess(event, this.inflation);
  }
}
@Component({
  selector: 'ff-price-admin',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './price.component.html'
})
export class PriceComponent extends ConversionBaseComponent implements OnInit {
  constructor(private admin: Admin, eventStore: EventStore, dialog: MatDialog, cdr: ChangeDetectorRef) {
    super(eventStore, dialog, cdr);
  }
  @Input() public option!: IOption;
  @Output() public priced: EventEmitter<void> = new EventEmitter();

  public timestamp!: UntypedFormControl;
  public newInvested!: UntypedFormControl;
  public newCash!: UntypedFormControl;
  public formGroup!: UntypedFormGroup;
  public ngOnInit(): void {
    this.timestamp = new UntypedFormControl(new Date().toISOString());
    this.newInvested = new UntypedFormControl(this.option.invested_amount);
    this.formGroup = new UntypedFormGroup({
      timestamp: this.timestamp,
      newInvested: this.newInvested
    });
    this.enabled = true;
  }

  public async priceInfo() {
    let event = {
      type: 'PRICE_INFO',
      timestamp: this.timestamp.value,
      option: this.option.code,
      invested_amount: this.newInvested.value
    }
    await this.importAndProcess(event, this.priced);
  }
}
