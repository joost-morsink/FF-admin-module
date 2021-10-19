import { Component, Output, ViewChild, Input } from '@angular/core';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption, IEventNewOption, IValidationMessage, IEventStatistics, IFullEvent } from '../interfaces/interfaces';
import { EventEmitter } from 'protractor';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';

@Component({
  selector: 'ff-events',
  templateUrl: './events.component.html'
})
export class EventsComponent {
  constructor(private admin: Admin, private eventStore: EventStore) {
    this.stats = {
      processed: 0, unprocessed: 0, firstUnprocessed: new Date(), lastProcessed: new Date()
    };
    this.unimported = [];
    this.fetchStats();
    this.fetchUnimported();
  }
  public stats: IEventStatistics;
  public unimported: string[];
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
