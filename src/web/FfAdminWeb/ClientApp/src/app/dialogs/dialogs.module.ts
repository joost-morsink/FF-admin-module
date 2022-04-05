import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FfUiModule } from '../ffUi.module';
import { ErrorDialog } from './error.dialog';
import { InfoDialog } from './info.dialog';

@NgModule({
    declarations: [
        ErrorDialog,
        InfoDialog
    ],
    imports: [
        CommonModule,
        FfUiModule
    ]
})
export class DialogsModule {
}