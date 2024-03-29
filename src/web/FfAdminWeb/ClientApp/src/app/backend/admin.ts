import { Component, Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  IOption,
  ICharity,
  IOpenTransfer,
  IAuditInfo,
  IDonationsByCurrency,
  IFractionSpec
} from '../interfaces/interfaces';

@Injectable()
export class Admin {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }

  public async getOptions(): Promise<IOption[]> {
    return this.http.get<IOption[]>(this.baseUrl + "admin/options").toPromise();
  }

  public async getOption(optionId: string): Promise<IOption> {
    return this.http.get<IOption>(this.baseUrl + `admin/options/${optionId}`).toPromise();
  }

  public async getCharities(): Promise<ICharity[]> {
    return this.http.get<ICharity[]>(this.baseUrl + "admin/charities").toPromise();
  }
  public async getCharityPartitions(charityId: string): Promise<IFractionSpec[]> {
    return this.http.get<IFractionSpec[]>(this.baseUrl + `admin/charities/${charityId}/partitions`).toPromise();
  }
  public async getOpenTransfers(): Promise<IOpenTransfer[]> {
    return this.http.get<IOpenTransfer[]>(this.baseUrl + "admin/charities/opentransfers").toPromise();
  }

  public async importBankTransfers(camtFile: File): Promise<void> {
    const formData = new FormData();
    formData.append("file", camtFile);

    return this.http.post<void>(this.baseUrl + "admin/charities/opentransfers/resolve/camt", formData).toPromise();
  }

  public async getAuditReports(): Promise<IAuditInfo[]> {
    return this.http.get<IAuditInfo[]>(this.baseUrl + "admin/audit/all").toPromise();
  }

  public async getDonationsByCurrency(): Promise<IDonationsByCurrency[]> {
    return this.http.get<IDonationsByCurrency[]>(this.baseUrl + "admin/donations/bycurrency").toPromise();
  }

  public async calculateExit(option: IOption, extra_cash: number, invested: number, timestamp: string) {
    let res: number = await this.http.get<number>(this.baseUrl + `admin/calculation/exit?option=${option.code}&invested=${invested}&extra_cash=${extra_cash}&timestamp=${timestamp}`).toPromise();
    return res || 0;
  }
  public async getLoanableCash(option: IOption, at: Date) {
    let res: number = await this.http.get<number>(this.baseUrl + `admin/options/${option.code}/loanable-cash?at=${at.toISOString()}`).toPromise();
    return res || 0;
  }
  public async recreateDatabase(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "admin/database/recreate", {}).toPromise();
  }

  public async updateDatabase(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "admin/database/update", {}).toPromise();
  }

}
