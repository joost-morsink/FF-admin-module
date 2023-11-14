import {Component, Inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {IEvent, IEventStatistics, IFullEvent} from '../interfaces/interfaces';

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

  public audit(): Promise<void> {
    return this.http.post<void>(this.baseUrl + "eventstore/audit", {}).toPromise();
  }

  public getBranches(): Promise<string[]> {
    return this.http.get<string[]>(this.baseUrl + "eventstore/branches").toPromise();
  }

  public getEvents(skip: number, limit?: number): Promise<IFullEvent[]> {
    let args = [];
    if (skip > 0) {
      args.push(`skip=${skip}`);
    }
    if (limit !== null && limit !== undefined){
      args.push(`limit=${limit}`);
    }
    let querystring = args.length == 0 ? "" : "?" + args.join("&");
    return this.http.get<IFullEvent[]>(this.baseUrl + `eventstore/events${querystring}`).toPromise();
  }
  public importEvents(events: IFullEvent[]): Promise<void> {
    return this.http.post<void>(this.baseUrl + `eventstore/import-many`, events).toPromise();
  }
  public branch(branchName: string): Promise<void> {
    return this.http.post<void>(this.baseUrl + `eventstore/branch`, { to: branchName }).toPromise();
  }

  public newBranch(branchName: string): Promise<void> {
    return this.http.post<void>(this.baseUrl + `eventstore/new-branch`, { name: branchName }).toPromise();
  }

  public fastForward(branchName: string): Promise<void> {
    return this.http.post<void>(this.baseUrl + `eventstore/fast-forward`, { name: branchName }).toPromise();
  }

  public rebase(branchName: string): Promise<void> {
    return this.http.post<void>(this.baseUrl + `eventstore/rebase`, { on: branchName }).toPromise();
  }

  public removeBranch(branchName: string): Promise<void> {
    return this.http.delete<void>(this.baseUrl + `eventstore/branch/${branchName}`).toPromise();
  }

}
