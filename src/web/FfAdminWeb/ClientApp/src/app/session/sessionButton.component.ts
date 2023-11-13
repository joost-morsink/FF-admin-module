import {Component} from '@angular/core';
import {EventStore} from '../backend/eventstore';
import {MatDialog} from '@angular/material/dialog';
import {ErrorDialog} from '../dialogs/error.dialog';
import {CurrentBranch} from "../currentBranch";
import {SelectBranchDialog} from "./select-branch.dialog";

@Component({
  selector: 'session-button',
  templateUrl: './sessionButton.component.html'
})
export class SessionButtonComponent {
  constructor(private eventStore: EventStore, private currentBranch: CurrentBranch, private dialog: MatDialog) {
    this.cls = 'btn-success';
    this.text = this.currentBranch.getBranchName();

  }

  cls = 'btn-light';
  text = 'Loading...';
  enabled= true;
  private setBranch(branch: string){
    this.currentBranch.setBranchName(branch);
    this.text = branch;
    this.refresh();
  }
  private refresh() {
    location.reload();
  }
  public async onClick(): Promise<void> {
    try {
      this.enabled=false;
      let dlg = this.dialog.open(SelectBranchDialog);
      let result = await dlg.beforeClosed().toPromise();
      console.log(result);
      let branch = dlg.componentInstance.getBranchName();

      switch(result){
        case 'switch':
          this.setBranch(branch);
          break;
        case 'branch':
          await this.eventStore.branch(branch);
          this.setBranch(branch);
          break;
        case 'rebase':
          await this.eventStore.rebase(branch);
          this.refresh();
          break;
        case 'forward':
          await this.eventStore.fastForward(branch);
          this.refresh();
          break;
      }
    } catch (ex) {
      console.log(ex);
      let errs = ex.error || [{key: "main", message: ex.message}];
      this.dialog.open(ErrorDialog, {
        data: {errors: errs}
      });
    } finally {
      this.enabled = true;
    }
  }
}
