import {Component, Input, ChangeDetectionStrategy, ChangeDetectorRef} from '@angular/core';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { ICharity, IEventStatistics, IFullEvent, IOption } from '../interfaces/interfaces';
import { MatDialog } from '@angular/material/dialog';
import { InfoDialog } from '../dialogs/info.dialog';
import { ErrorDialog } from '../dialogs/error.dialog';

@Component({
  selector: 'ff-events',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './events.component.html'
})
export class EventsComponent {
  constructor(private admin: Admin, private eventStore: EventStore, private dialog: MatDialog, private cdr: ChangeDetectorRef) {
    this.stats = {
      processed: 0, unprocessed: 0, firstUnprocessed: new Date(), lastProcessed: new Date()
    };
    this.fetchStats();
  }
  public stats: IEventStatistics;
  public enabled: boolean = true;

  public async executeDisabled<T>(f: () => Promise<T>): Promise<T> {
    try {
      this.enabled = false;
      return await f();
    } finally {
      this.enabled = true;
    }
  }
  public async fetchStats(): Promise<void> {
    let stats = await this.eventStore.getStatistics();
    this.stats = stats;
    this.cdr.detectChanges();
  }
  public async audit() {
    await this.executeDisabled(async () => {
      try {
        await this.eventStore.audit();
        await this.fetchStats();
        this.dialog.open(InfoDialog, {
          data: { message: "Success!" }
        });
      }
      catch (ex: any) {
        this.dialog.open(ErrorDialog, {
          data: { errors: ex.error }
        });
      }
    });
  }
  public async export() {
    await this.executeDisabled(async () => {
      try {
        let events = await this.eventStore.getEvents(0);

        let content = events.map(e => JSON.stringify(e)).join("\r\n");
        content  = content.replace(/[\u007F-\uFFFF]/g, chr =>
           "\\u" + (chr.charCodeAt(0).toString(16)).padStart(4, '0').toUpperCase());

        let encoded = btoa(content);
        let url = `data:application/json;base64,${encoded}`;
        let a = document.createElement('a');
        a.href = url;
        a.download = "events.json";
        a.click();
      }
      catch (ex: any) {
        console.log(ex);
        this.dialog.open(ErrorDialog, {
          data: { errors: ex.error }
        });
      }
    });
  }

}

@Component({
  selector: 'ff-event-stats',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './eventStats.component.html'
})
export class EventStatsComponent {
  constructor() {
  }

  @Input() public stats!: IEventStatistics;
}

@Component({
  selector: 'ff-event-tile',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './eventTile.component.html'
})
export class EventTileComponent {
  constructor() {
  }
  @Input() public data!: IFullEvent;
  public getClass() {
    return this.data.type.toLowerCase().split('_').join('-');
  }
  public getDate(): string {
    return this.data.timestamp ? new Date(this.data.timestamp).toLocaleString() : '';
  }
}

@Component({
  selector: 'ff-event-list',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './eventList.component.html'
})
export class EventListComponent {
  constructor(public eventStore: EventStore, public admin: Admin, private cdr: ChangeDetectorRef) {
    this.init();
  }
  public data: IFullEvent[] = [];
  public async fetch() : Promise<IFullEvent[]> {
    let charities = (await this.admin.getCharities()).reduce<Record<string, ICharity>>((acc,x) => {
      acc[x.code] = x;
      return acc; }, {});
    let options = (await this.admin.getOptions()).reduce<Record<string, IOption>>((acc,x) => {
      acc[x.code]=x;
      return acc; }, {});
    let events = await this.eventStore.getEvents(0, -60);
    for(let e of events) {
      if(e.charity in charities)
        e.charity = charities[e.charity].name;
      if(e.option in options)
        e.exchanged_currency = options[e.option].currency;
    }
    return events;
  }
  public async init() : Promise<void> {
    let results = await this.fetch();
    this.data=results.reverse();
    this.cdr.detectChanges();
  }
}
