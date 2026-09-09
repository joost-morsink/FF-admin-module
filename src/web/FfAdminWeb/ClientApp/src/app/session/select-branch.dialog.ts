import {Component, EventEmitter, Input, Output, ChangeDetectionStrategy, ChangeDetectorRef} from "@angular/core";
import {UntypedFormControl} from "@angular/forms";
import {EventStore} from "../backend/eventstore";

@Component({
  selector: 'ff-select-branch',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './select-branch.dialog.html'
})
export class SelectBranchDialog {
  constructor(private eventStore: EventStore, private cdr: ChangeDetectorRef) {
    this.branchName = new UntypedFormControl('');
    void this.load();
  }

  public branchName: UntypedFormControl;
  public branches: string[] = [];
  public enabled: boolean = true;

  public getBranchName(): string {
    return this.branchName.value || '';
  }

  public async load(): Promise<void> {
    try {
      this.enabled = false;
      this.branches = await this.eventStore.getBranches();
    } finally {
      this.enabled = true;
      this.cdr.detectChanges();
    }
  }

  public branchClicked(branch: string) {
    this.branchName.setValue(branch);
  }
}

@Component(
  {
    selector: 'branch-button',
    standalone: false,
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './branch-button.component.html'
  }
)
export class BranchButton {
  @Input() public branch = '';
  @Input() public enabled = false;
  @Output() public clickBranch = new EventEmitter<string>();

  public click() {
    if(this.enabled) {
      this.clickBranch.emit(this.branch);
    }
  }
}
