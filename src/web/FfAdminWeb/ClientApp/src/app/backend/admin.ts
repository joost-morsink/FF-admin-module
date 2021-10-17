import { Component, Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IOption, ICharity} from '../interfaces/interfaces';

@Injectable()
export class Admin {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }
  public async getOptions(): Promise<IOption[]> {
    return this.http.get<IOption[]>(this.baseUrl + "admin/options").toPromise();
  }
  public async getCharities(): Promise<ICharity[]> {
    return this.http.get<ICharity[]>(this.baseUrl + "admin/charities").toPromise();
  }

  public async calculateExit(option: IOption, invested: number, timestamp: string) {
    var res : number = await this.http.get<number>(this.baseUrl + `admin/calculation/exit?option=${option.id}&invested=${invested}&timestamp=${timestamp}`).toPromise();
    return res || 0;
  }
}
