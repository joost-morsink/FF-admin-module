import {Component} from "@angular/core";
import {UntypedFormControl} from "@angular/forms";

@Component({
  selector: 'ff-commit-dialog',
  templateUrl: './commit.dialog.html'
})
export class CommitDialog {
  constructor() {
    this.message = new UntypedFormControl();
  }

  public message: UntypedFormControl;

  public getMessage(): string {
    return this.message.value;
  }
}

