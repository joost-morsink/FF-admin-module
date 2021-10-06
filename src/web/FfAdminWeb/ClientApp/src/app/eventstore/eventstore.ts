import { Component, Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IEvent, IEventStatistics } from '../interfaces/interfaces';

@Injectable()
export class EventStore {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }

  public isSessionAvailable(): Promise<boolean> {
    return this.http.get<boolean>(this.baseUrl + "eventstore/session/is-available").toPromise();
  }

  public async startSession(): Promise<boolean> {
    await this.http.post(this.baseUrl + "eventstore/session/start", {}).toPromise();
    return true;
  }

  public async endSession(message: string): Promise<boolean> {
    console.log(`ending session with ${message}`);
    await this.http.post(this.baseUrl + "eventstore/session/stop", { message: message }).toPromise();
    return true;
  }
  public async postEvent(ev: IEvent) {
    await this.http.post<void>(this.baseUrl + "eventstore/import", ev).toPromise();
  }
  public getStatistics(): Promise<IEventStatistics> {
    return this.http.get<IEventStatistics>("eventstore/statistics/main").toPromise();
  }
  public process(): Promise<void> {
    return this.http.post<void>("eventstore/process", {}).toPromise();
  }
  public reset(): Promise<void> {
    return this.http.post<void>("eventstore/reset", {}).toPromise();
  }
  public deleteAll(): Promise<void> {
    return this.http.post<void>("eventstore/deleteAll", {}).toPromise();
  }
}
