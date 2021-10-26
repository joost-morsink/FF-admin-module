import { Component } from '@angular/core';
import { EventStore } from './backend/eventstore';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';
  constructor(private eventStore: EventStore) {
    window.onbeforeunload = e => this.closing(e);
  }
  private closing(e: any) {
    if (this.eventStore.wasSessionAvailable()) {
      e.preventDefault();
      e.returnValue = '';
    } else {
      delete e['returnValue'];
    }
  }
}
