import { Component } from '@angular/core';
import { EventStore } from '../eventstore/eventstore';

@Component({
  selector: 'session-button',
  templateUrl: './sessionButton.component.html'
})
export class SessionButtonComponent {
  constructor(private eventStore: EventStore) {
    this.fetchAndSetAvailability();
  }
  private async fetchAndSetAvailability(): Promise<void> {
    this.eventStore.isSessionAvailable().then(avail => this.setAvailability(avail));
  }
  private setAvailability(avail: boolean): void {
    this.available = avail;
    if (avail) {
      this.cls = 'btn-success';
      this.text = 'End session';
    } else {
      this.cls = 'btn-danger';
      this.text = 'Start session';
    }
  }
  available = undefined;
  cls = 'btn-light';
  text = 'Loading...';
  enabled = true;
  public async onClick(): Promise<void> {
    try {
      this.enabled = false;
      console.log(this.available);
      if (!this.available)
        await this.eventStore.startSession();
      else {
        await this.eventStore.endSession("All work is done");
      }
    } catch (ex) {
      throw ex;
    } finally {
      this.enabled = true;
      await this.fetchAndSetAvailability();
    }
  }
}