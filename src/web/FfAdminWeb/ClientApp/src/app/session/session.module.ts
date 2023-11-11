import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { SessionButtonComponent } from './sessionButton.component';
import {SelectBranchDialog} from "./select-branch.dialog";

@NgModule({
    declarations: [
        SessionButtonComponent,
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

