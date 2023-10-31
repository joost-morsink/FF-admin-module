import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { SessionButtonComponent } from './sessionButton.component';
import {CommitDialog} from "./commit-dialog.component";
import {SelectBranchDialog} from "./select-branch.dialog";

@NgModule({
    declarations: [
        SessionButtonComponent,
        CommitDialog,
      SelectBranchDialog
    ],
    imports: [
        CommonModule,
        FfUiModule
    ],
    exports: [
        SessionButtonComponent
    ]
})
export class SessionModule {
}

