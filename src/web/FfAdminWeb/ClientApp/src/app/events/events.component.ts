import { Component, Output, ViewChild, Input } from '@angular/core';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption, IEventNewOption, IValidationMessage, IEventStatistics, IFullEvent, IRemoteStatus } from '../interfaces/interfaces';
import { EventEmitter } from 'protractor';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { InfoDialog } from '../dialogs/info.dialog';

@Component({
  selector: 'ff-events',
  templateUrl: './events.component.html'
})
export class EventsComponent {
  constructor(private admin: Admin, private eventStore: EventStore, private dialog: MatDialog) {
    this.stats = {
      processed: 0, unprocessed: 0, firstUnprocessed: new Date(), lastProcessed: new Date()
    };
    this.unimported = [];
    this.fetchStats();
    this.fetchUnimported();
    this.fetchRemoteStatus();
  }
  public stats: IEventStatistics;
  public unimported: string[];
  public remote: IRemoteStatus;
  public enabled: boolean = true;
  public unprocessedEvents: IFullEvent[];

  public async executeDisabled<T>(f: () => Promise<T>): Promise<T> {
    try {
      this.enabled = false;
      return await f();
    } finally {
      this.enabled = true;
    }
  }
  public async import(): Promise<void> {
    return await this.executeDisabled(async () => {
      let res = await this.eventStore.import(this.unimported);
      await this.fetchUnimported();
      await this.fetchStats();
      this.unprocessedEvents = null;
      return res;
    });
  }
  public async process(): Promise<void>{
    return await this.executeDisabled(async () => {
      let res = await this.eventStore.process();
      await this.fetchStats();
      this.unprocessedEvents = null;
      return res;
    });
  }
  public async reset(): Promise<void> {
    return await this.executeDisabled(async () => {
      let res = await this.eventStore.reset();
      await this.fetchStats();
      this.unprocessedEvents = null;
      return res;
    });
  }
  public async clear(): Promise<void> {
    return await this.executeDisabled(async () => {
      let res = await this.eventStore.deleteAll();
      await this.fetchStats();
      await this.fetchUnimported();
      this.unprocessedEvents = null;
      return res;
    });
  }
  public async fetchStats(): Promise<void> {
    let stats = await this.eventStore.getStatistics();
    this.stats = stats;
  }
  public async fetchUnimported(): Promise<void> {
    this.unimported = await this.eventStore.getUnimported();
  }
  public async fetchUnprocessed(): Promise<void> {
    this.unprocessedEvents = await this.eventStore.getUnprocessed();
  }
  public async fetchRemoteStatus(): Promise<void> {
    this.remote = await this.eventStore.getRemoteStatus();
  }
  public async pull() {
    await this.eventStore.pull();
    this.dialog.open(InfoDialog, {
      data: { message: "Pulled successfully!" }
    });
    this.fetchUnimported();
    this.fetchRemoteStatus();
  }
  public async push() {
    await this.eventStore.push();
    this.dialog.open(InfoDialog, {
      data: { message: "Pushed successfully!" }
    });
    this.fetchRemoteStatus();
  }
}

@Component({
  selector: 'ff-event-stats',
  templateUrl: './eventStats.component.html'
})
export class EventStatsComponent {
  constructor() {
  }

  @Input() public stats: IEventStatistics;
}

@Component({
  selector: 'ff-event-unimported',
  templateUrl: './eventUnimported.component.html'
})
export class EventUnimportedComponent {
  constructor() {
    this.unimported = [];
  }
  @Input() public unimported: string[];
}

@Component({
  selector: 'ff-event-tile',
  templateUrl: './eventTile.component.html'
})
export class EventTileComponent {
  constructor() {
  }
  @Input() public data: IFullEvent;
  public getClass() {
    return this.data.type.toLowerCase().split('_').join('-');
  }
  public getDate(): string {
    return new Date(this.data.timestamp).toLocaleString();
  }
}
