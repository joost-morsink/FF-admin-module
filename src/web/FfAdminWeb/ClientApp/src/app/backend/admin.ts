import { Component, Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
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
    return firstValueFrom(this.http.get<IOption[]>(this.baseUrl + "admin/options"));
  }

  public async getOption(optionId: string): Promise<IOption> {
    return firstValueFrom(this.http.get<IOption>(this.baseUrl + `admin/options/${optionId}`));
  }

  public async getCharities(): Promise<ICharity[]> {
    return firstValueFrom(this.http.get<ICharity[]>(this.baseUrl + "admin/charities"));
  }
  public async getCharityPartitions(charityId: string): Promise<IFractionSpec[]> {
    return firstValueFrom(this.http.get<IFractionSpec[]>(this.baseUrl + `admin/charities/${charityId}/partitions`));
  }
  public async getOpenTransfers(): Promise<IOpenTransfer[]> {
    return firstValueFrom(this.http.get<IOpenTransfer[]>(this.baseUrl + "admin/charities/opentransfers"));
  }

  public async importBankTransfers(camtFile: File): Promise<void> {
    const formData = new FormData();
    formData.append("file", camtFile);

    return firstValueFrom(this.http.post<void>(this.baseUrl + "admin/charities/opentransfers/resolve/camt", formData));
  }

  public async getAuditReports(): Promise<IAuditInfo[]> {
    return firstValueFrom(this.http.get<IAuditInfo[]>(this.baseUrl + "admin/audit/all"));
  }

  public async getDonationsByCurrency(): Promise<IDonationsByCurrency[]> {
    return firstValueFrom(this.http.get<IDonationsByCurrency[]>(this.baseUrl + "admin/donations/bycurrency"));
  }

  public async calculateExit(option: IOption, extra_cash: number, invested: number, timestamp: string) {
    const res = await firstValueFrom(this.http.get<number>(this.baseUrl + `admin/calculation/exit?option=${option.code}&invested=${invested}&extra_cash=${extra_cash}&timestamp=${timestamp}`));
    return res ?? 0;
  }
  public async getLoanableCash(option: IOption, at: Date) {
    const res = await firstValueFrom(this.http.get<number>(this.baseUrl + `admin/options/${option.code}/loanable-cash?at=${at.toISOString()}`));
    return res ?? 0;
  }
  public async recreateDatabase(): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.baseUrl + "admin/database/recreate", {}));
  }

  public async updateDatabase(): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.baseUrl + "admin/database/update", {}));
  }

}
