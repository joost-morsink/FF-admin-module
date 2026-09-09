import {Component, Inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import {IEvent, IEventStatistics, IFullEvent} from '../interfaces/interfaces';

@Injectable()
export class EventStore {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }

  public async postEvent(ev: IEvent) {
    await firstValueFrom(this.http.post<void>(this.baseUrl + "eventstore/import", ev));
  }

  public getStatistics(): Promise<IEventStatistics> {
    return firstValueFrom(this.http.get<IEventStatistics>(this.baseUrl + "eventstore/statistics/main"));
  }

  public importCsv(formData: FormData) {
    return firstValueFrom(this.http.post<void>(this.baseUrl + "eventstore/donations/give", formData));
  }

  public audit(): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.baseUrl + "eventstore/audit", {}));
  }

  public getBranches(): Promise<string[]> {
    return firstValueFrom(this.http.get<string[]>(this.baseUrl + "eventstore/branches"));
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
    return firstValueFrom(this.http.get<IFullEvent[]>(this.baseUrl + `eventstore/events${querystring}`));
  }
  public importEvents(events: IFullEvent[]): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.baseUrl + `eventstore/import-many`, events));
  }
  public branch(branchName: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.baseUrl + `eventstore/branch`, { to: branchName }));
  }

  public newBranch(branchName: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.baseUrl + `eventstore/new-branch`, { name: branchName }));
  }

  public fastForward(branchName: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.baseUrl + `eventstore/fast-forward`, { name: branchName }));
  }

  public rebase(branchName: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(this.baseUrl + `eventstore/rebase`, { on: branchName }));
  }

  public removeBranch(branchName: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(this.baseUrl + `eventstore/branch/${branchName}`));
  }

}
