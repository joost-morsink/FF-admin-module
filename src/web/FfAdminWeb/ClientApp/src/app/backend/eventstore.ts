import { Component, Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IEvent, IEventStatistics, IFullEvent, IRemoteStatus } from '../interfaces/interfaces';

@Injectable()
export class EventStore {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }
  private lastSessionAvailable: boolean;

  public wasSessionAvailable(): boolean {
    return this.lastSessionAvailable;
  }
  public async isSessionAvailable(): Promise<boolean> {
    return this.lastSessionAvailable = await this.http.get<boolean>(this.baseUrl + "eventstore/session/is-available").toPromise();
  }

  public async startSession(): Promise<boolean> {
    await this.http.post(this.baseUrl + "eventstore/session/start", {}).toPromise();
    this.lastSessionAvailable = true;
    return true;
  }

  public async endSession(message: string): Promise<boolean> {
    console.log(`ending session with ${message}`);
    await this.http.post(this.baseUrl + "eventstore/session/stop", { message: message }).toPromise();
    this.lastSessionAvailable = false;
    return true;
  }
  public async postEvent(ev: IEvent) {
    await this.http.post<void>(this.baseUrl + "eventstore/import", ev).toPromise();
  }
  public getStatistics(): Promise<IEventStatistics> {
    return this.http.get<IEventStatistics>(this.baseUrl + "eventstore/statistics/main").toPromise();
  }
  public process(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "eventstore/process", {}).toPromise();
  }
  public reset(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "eventstore/reset", {}).toPromise();
  }
  public deleteAll(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "eventstore/deleteAll", {}).toPromise();
  }
  public getUnimported(): Promise<string[]> {
    return this.http.get<string[]>(this.baseUrl + "eventstore/files/unimported").toPromise();
  }
  public getUnprocessed(): Promise<IFullEvent[]> {
    return this.http.get<IFullEvent[]>(this.baseUrl + "eventstore/unprocessed").toPromise();
  }
  public import(files: string[]): Promise<void> {
    return this.http.post<void>(this.baseUrl + "eventstore/files/import", files).toPromise();
  }
  public importCsv(formData: FormData) {
    return this.http.post<void>(this.baseUrl + "eventstore/donations/give", formData).toPromise();
  }
  public getRemoteStatus(): Promise<IRemoteStatus> {
    return this.http.get<IRemoteStatus>(this.baseUrl + "eventstore/remote/status").toPromise();
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
