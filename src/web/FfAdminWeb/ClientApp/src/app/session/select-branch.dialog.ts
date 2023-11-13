import {Component, EventEmitter, Input, Output} from "@angular/core";
import {FormControl, UntypedFormControl} from "@angular/forms";
import {EventStore} from "../backend/eventstore";

@Component({
  selector: 'ff-select-branch',
  templateUrl: './select-branch.dialog.html'
})
export class SelectBranchDialog {
  constructor(private eventStore: EventStore) {
    this.branchName = new UntypedFormControl();
    let _ = this.load();
  }

  public branchName: FormControl<string>;
  public branches: string[] = [];
  public enabled: boolean = true;

  public getBranchName(): string {
    return this.branchName.value;
  }

  public async load(): Promise<void> {
    try {
      this.enabled = false;
      this.branches = await this.eventStore.getBranches();
    } finally {
      this.enabled = true;
    }
  }

  public branchClicked(branch: string) {
    this.branchName.setValue(branch);
  }
}

@Component(
  {
    selector: 'branch-button',
    templateUrl: './branch-button.component.html'
  }
)
export class BranchButton {
  @Input() public branch: string;
  @Input() public enabled: boolean;
  @Output() public clickBranch = new EventEmitter<string>();

  public click() {
    if(this.enabled) {
      this.clickBranch.emit(this.branch);
    }
  }
}
