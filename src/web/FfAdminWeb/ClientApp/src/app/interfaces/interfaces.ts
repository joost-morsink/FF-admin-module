
export interface IEvent {
  Type: string;
  Timestamp: Date;
}
export interface INewOption extends IEvent {
  Code: string;
  Name: string;
  Currency: string;
  Reinvestment_fraction: number;
  FutureFund_fraction: number;
  Charity_fraction: number;
  Bad_year_fraction: number;
}

export interface INewCharity extends IEvent {
  Code: string;
  Name: string;
}
