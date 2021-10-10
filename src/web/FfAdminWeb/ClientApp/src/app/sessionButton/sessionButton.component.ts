import { Component, Inject } from '@angular/core';
import { EventStore } from '../eventstore/eventstore';
import { MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormControl } from '@angular/forms';
import { ErrorDialog } from '../error/error.dialog';

@Component({
  selector: 'session-button',
  templateUrl: './sessionButton.component.html'
})
export class SessionButtonComponent {
  constructor(private eventStore: EventStore, private dialog: MatDialog) {
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
        let dlg = this.dialog.open(CommitDialog);
        let result = await dlg.beforeClosed().toPromise();
        if (result) {
          let msg = dlg.componentInstance.getMessage() || "All work is done";
          await this.eventStore.endSession(msg);
        }
      }
    } catch (ex) {
      let errs = ex.error || [{ key: "main", message: ex.message }];
      this.dialog.open(ErrorDialog, {
        data: { errors: errs}
      });
    } finally {
      this.enabled = true;
      await this.fetchAndSetAvailability();
    }
  }
}

@Component({
  selector: 'ff-commit-dialog',
  templateUrl: './commit.dialog.html'
})
export class CommitDialog {
  constructor() {
    this.message = new FormControl();
  }
  public message: FormControl;
  public getMessage(): string {
    return this.message.value;
  }
}