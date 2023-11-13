import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FfUiModule} from '../ffUi.module';
import {SessionButtonComponent} from './sessionButton.component';
import {BranchButton, SelectBranchDialog} from "./select-branch.dialog";

@NgModule({
  declarations: [
    SessionButtonComponent,
    SelectBranchDialog,
    BranchButton
  ],
  imports: [
    CommonModule,
    FfUiModule
  ],
  exports: [
    SessionButtonComponent,
    BranchButton
  ]
})
export class SessionModule {
}

