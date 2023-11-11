import { Component, Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IEvent, IEventStatistics, IFullEvent } from '../interfaces/interfaces';

@Injectable()
export class EventStore {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }

  public async postEvent(ev: IEvent) {
    await this.http.post<void>(this.baseUrl + "eventstore/import", ev).toPromise();
  }
  public getStatistics(): Promise<IEventStatistics> {
    return this.http.get<IEventStatistics>(this.baseUrl + "eventstore/statistics/main").toPromise();
  }
  public importCsv(formData: FormData) {
    return this.http.post<void>(this.baseUrl + "eventstore/donations/give", formData).toPromise();
  }
  public pull(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "eventstore/remote/pull", {}).toPromise();
  }
  public push(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "eventstore/remote/push", {}).toPromise();
  }
  public audit(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "eventstore/audit", {}).toPromise();
  }
}
