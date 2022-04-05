import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { SessionButtonComponent, CommitDialog } from './sessionButton.component';

@NgModule({
    declarations: [
        SessionButtonComponent,
        CommitDialog
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