
export interface IEvent {
  type: string;
  timestamp?: Date;
}
export interface ICode {
  id?: number;
  code: string;
}
export interface IName {
  name: string;
}
export interface IOptionFractions {
  reinvestment_fraction: number;
  futureFund_fraction: number;
  charity_fraction: number;
  bad_year_fraction: number;
}
export interface IOption extends ICode, IName, IOptionFractions{
  currency: string;
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
export interface IEventNewOption extends IEvent, IOption {
  
}

export interface IEventNewCharity extends IEvent, ICode, IName {

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