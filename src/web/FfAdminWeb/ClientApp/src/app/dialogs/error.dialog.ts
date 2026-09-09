import { Component, Inject, ChangeDetectionStrategy } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { IValidationMessage } from '../interfaces/interfaces';
@Component({
  selector: 'ff-error-dialog',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './error.dialog.html'
})
export class ErrorDialog {
  constructor(@Inject(MAT_DIALOG_DATA) public data: { errors: IValidationMessage[] }) {
  }
} 