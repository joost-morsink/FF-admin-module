
export interface IEvent {
  type: string;
  timestamp?: Date;
}
export interface ICode {
  code: string;
}
export interface IName {
  name: string;
}
export interface ICurrency {
  currency: string;
}
export interface IOptionFractions {
  reinvestment_fraction: number;
  futureFund_fraction: number;
  charity_fraction: number;
  bad_year_fraction: number;
}

export interface IOptionAmounts {
  invested_amount: number;
  cash_amount?: number;
}
export interface IOption extends ICode, IName, ICurrency, IOptionFractions, IOptionAmounts{
}
export interface IFullEvent extends IEvent, IName, IOptionFractions, IOptionAmounts {
  option_currency: string;
  charity: string;
  option: string;
  currency: string;
  amount: number;
  exchanged_currency: string;
  exchanged_amount: number;
  inflation_factor: number;
}

export interface IBank {
  bank_name: string;
  bank_account_no: string;
  bank_bic: string;
}
export interface ICharity extends ICode, IName, IBank {

}
export interface IOptionUpdate extends ICode, IOptionFractions {

}
export interface IEventNewOption extends IEvent, ICode, IName, ICurrency, IOptionFractions {

}

export interface IEventNewCharity extends IEvent, ICode, IName {

}

export interface IEventLiquidate extends IEvent, IOptionAmounts {
  option: string;
  transaction_reference: string;
}

export interface IFractionSpec {
  holder: string;
  fraction: number;
}
export interface IEventCharityPartition extends IEvent {
  charity: string;
  partitions: IFractionSpec[];
}
export interface IValidationMessage {
  key: string;
  message: string;
}
export interface IEventStatistics {
  processed: number;
  lastProcessed: Date;
  unprocessed: number;
  firstUnprocessed: Date;
}
export interface IOpenTransfer {
  charity: string;
  name: string;
  currency: string;
  amount: number;
}
export interface IAuditInfo {
  timestamp: Date;
  hashCode: string;
  eventCount: number;
  previousHashCode?: string;
  previousCount?: number;
}
export interface IDonationsByCurrency {
  currency: string;
  amount: number;
  worth: number;
  allocated: number;
  transferred: number;
}
