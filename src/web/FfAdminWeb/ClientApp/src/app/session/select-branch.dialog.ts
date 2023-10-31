import {Component} from "@angular/core";
import {FormControl, UntypedFormControl} from "@angular/forms";

@Component({
  selector: 'ff-select-branch',
  templateUrl: './select-branch.dialog.html'
})
export class SelectBranchDialog {
  constructor() {
    this.branchName = new UntypedFormControl();
  }

  public branchName: FormControl<string>;

  public getBranchName(): string {
    return this.branchName.value;
  }
}