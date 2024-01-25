import { Component,  Input } from '@angular/core';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IEventStatistics, IFullEvent } from '../interfaces/interfaces';
import { MatDialog } from '@angular/material/dialog';
import { InfoDialog } from '../dialogs/info.dialog';
import { ErrorDialog } from '../dialogs/error.dialog';

@Component({
  selector: 'ff-events',
  templateUrl: './events.component.html'
})
export class EventsComponent {
  constructor(private admin: Admin, private eventStore: EventStore, private dialog: MatDialog) {
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
  }
  public async audit() {
    this.executeDisabled(async () => {
      try {
        await this.eventStore.audit();
        this.fetchStats();
        this.dialog.open(InfoDialog, {
          data: { message: "Success!" }
        });
      }
      catch (ex) {
        this.dialog.open(ErrorDialog, {
          data: { errors: ex.error }
        });
      }
    });
  }
  public async export() {
    this.executeDisabled(async () => {
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
      catch (ex) {
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
  templateUrl: './eventStats.component.html'
})
export class EventStatsComponent {
  constructor() {
  }

  @Input() public stats: IEventStatistics;
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
